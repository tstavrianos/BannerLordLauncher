using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using BannerLord.Common.Xml;
using Medallion.Collections;
using Mono.Cecil;
using Splat;
using Trinet.Core.IO.Ntfs;
using Alphaleonis.Win32.Filesystem;

namespace BannerLord.Common
{
    public sealed class ModManager : IModManager
    {
        private readonly IModManagerClient _client;

        private string _basePath;

        private string _modulePath;

        private readonly object _lock = new object();
        private bool _runValidation;

        public ModManager(IModManagerClient client)
        {
            this._client = client;
            this.Mods.CollectionChanged += this.Mods_CollectionChanged;
        }

        private void Mods_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Validate();
        }

        public ObservableCollection<ModEntry> Mods { get; } = new ObservableCollection<ModEntry>();

        public string GameExeFolder { get; private set; }

        public bool Initialize(string config, string game, out string errorMessage)
        {
            errorMessage = default;
            if (!this._client.CanInitialize(config, game)) return false;

            this._runValidation = false;
            this._basePath = config;
            if (!Directory.Exists(this._basePath))
            {
                errorMessage = $"{this._basePath} does not exist";
                this.Log().Error(errorMessage);
                return false;
            }

            try
            {
                if (!Directory.Exists(Path.Combine(this._basePath, "BannerLordLauncher Backups")))
                    Directory.CreateDirectory(Path.Combine(this._basePath, "BannerLordLauncher Backups"));
            }
            catch (Exception e)
            {
                this.Log().Error(e);
            }

            var launcherData = UserData.Load(this, Path.Combine(this._basePath, "LauncherData.xml")) ?? new UserData();
            this._modulePath = Path.Combine(game, "Modules");
            this.GameExeFolder = Path.Combine(game, "bin", "Win64_Shipping_Client");
            var modulesFolder = Path.Combine(game, "Modules");
            if (!Directory.Exists(modulesFolder))
            {
                errorMessage = $"{modulesFolder} does not exist";
                this.Log().Error(errorMessage);
                return false;
            }

            var modules = Directory.EnumerateDirectories(modulesFolder, "*",System.IO.SearchOption.TopDirectoryOnly)
                .Select(dir => Module.Load(this, Path.GetFileName(dir), game)).Where(module => module != null).ToList();

            if (launcherData.SingleplayerData?.ModDatas != null)
            {
                foreach (var mod in launcherData.SingleplayerData.ModDatas)
                {
                    if (this.Mods.Any(x => x.UserModData.Id.Equals(mod.Id, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    var module = modules.FirstOrDefault(x => x.Id == mod.Id);
                    if (module == null)
                    {
                        this.Log().Warn($"{mod.Id} could not be found in {modulesFolder}");
                        continue;
                    }

                    modules.Remove(module);
                    var modEntry = new ModEntry {Module = module, UserModData = mod};
                    this.Mods.Add(modEntry);
                    if (modEntry.Module.Official) modEntry.IsChecked = true;
                }
            }

            foreach (var module in modules)
            {
                if (this.Mods.Any(x => x.Module.Id.Equals(module.Id, StringComparison.OrdinalIgnoreCase))) continue;
                var modEntry = new ModEntry {Module = module, UserModData = new UserModData(module.Id, false)};
                this.Mods.Add(modEntry);
                if (modEntry.Module.Official) modEntry.IsChecked = true;
            }

            this.AnalyzeAssemblies();
            this.BuildDependencies();
            this._runValidation = true;
            this.Validate();
            return true;
        }

        public bool Run(string gameExe, string extraGameArguments, out string errorMessage)
        {
            errorMessage = default;
            if (!this._client.CanRun(gameExe, extraGameArguments)) return false;

            gameExe ??= "Bannerlord.exe";
            var actualGameExe = Path.Combine(this.GameExeFolder, gameExe);
            if (string.IsNullOrEmpty(actualGameExe))
            {
                errorMessage = "Game executable could not be detected";
                this.Log().Error(errorMessage);
                return false;
            }

            if (!File.Exists(actualGameExe))
            {
                errorMessage = $"{actualGameExe} could not be found";
                this.Log().Error(errorMessage);
                return false;
            }

            foreach (var dll in this.GetAssemblies().Distinct())
                try
                {
                    var fi = new System.IO.FileInfo(dll);
                    if (!fi.Exists) continue;
                    try
                    {
                        if (!fi.AlternateDataStreamExists("Zone.Identifier")) continue;
                        var s = fi.GetAlternateDataStream("Zone.Identifier", System.IO.FileMode.Open);
                        s.Delete();
                    }
                    catch (Exception e)
                    {
                        this.Log().Error(e);
                    }
                }
                catch
                {
                    //
                }

            extraGameArguments ??= "";
            var args = extraGameArguments.Trim() + " " + this.GameArguments().Trim();
            this.Log().Warn($"Trying to execute: {actualGameExe} {args}");
            var info = new ProcessStartInfo
            {
                Arguments = args,
                FileName = actualGameExe,
                WorkingDirectory = Path.GetDirectoryName(actualGameExe) ?? throw new InvalidOperationException(),
                UseShellExecute = false
            };
            try
            {
                Process.Start(info);
            }
            catch (Exception e)
            {
                errorMessage = "Exception when trying to run the game. See the log for details";
                this.Log().Error(e);
                return false;
            }

            return true;
        }

        public bool OpenConfig(out string errorMessage)
        {
            errorMessage = default;
            try
            {
                if (string.IsNullOrEmpty(this._basePath) || !Directory.Exists(this._basePath))
                {
                    errorMessage = $"{this._basePath} is invalid";
                    return false;
                }

                var info = new ProcessStartInfo
                {
                    Arguments = this._basePath,
                    FileName = "explorer.exe",
                    WorkingDirectory = this._basePath,
                    UseShellExecute = false
                };
                Process.Start(info);
            }
            catch (Exception e)
            {
                this.Log().Error(e);
                errorMessage = e.Message;
                return false;
            }

            return true;
        }

        public bool Save(out string errorMessage)
        {
            errorMessage = default;
            if (!this._client.CanSave()) return false;
            var launcherDataFile = Path.Combine(this._basePath, "LauncherData.xml");
            var launcherData = UserData.Load(this, launcherDataFile) ?? new UserData();
            if (launcherData.SingleplayerData == null) launcherData.SingleplayerData = new UserGameTypeData();
            launcherData.SingleplayerData.ModDatas.Clear();
            foreach (var mod in this.Mods) launcherData.SingleplayerData.ModDatas.Add(mod.UserModData);

            this.BackupFile(launcherDataFile);
            try
            {
                launcherData.Save(launcherDataFile);
            }
            catch (Exception e)
            {
                errorMessage = "Exception when trying to save the mod list. See the log for details";
                this.Log().Error(e);
                return false;
            }

            this.AnalyzeAssemblies();

            return true;
        }

        public bool MoveToTop(int selectedIndex, out string errorMessage)
        {
            errorMessage = default;
            if (!this._client.CanMoveToTop(selectedIndex)) return false;
            if (selectedIndex <= 0 || selectedIndex >= this.Mods.Count)
            {
                errorMessage = "Index out of bounds";
                return false;
            }

            this.Mods.Move(selectedIndex, 0);
            this.Validate();
            return true;
        }

        public bool MoveUp(int selectedIndex, out string errorMessage)
        {
            errorMessage = default;
            if (!this._client.CanMoveUp(selectedIndex)) return false;
            if (selectedIndex <= 0 || selectedIndex >= this.Mods.Count)
            {
                errorMessage = "Index out of bounds";
                return false;
            }

            this.Mods.Move(selectedIndex, selectedIndex - 1);
            this.Validate();
            return true;
        }

        public bool MoveDown(int selectedIndex, out string errorMessage)
        {
            errorMessage = default;
            if (!this._client.CanMoveDown(selectedIndex)) return false;
            if (selectedIndex < 0 || selectedIndex >= this.Mods.Count - 1)
            {
                errorMessage = "Index out of bounds";
                return false;
            }

            this.Mods.Move(selectedIndex, selectedIndex + 1);
            this.Validate();
            return true;
        }

        public bool MoveToBottom(int selectedIndex, out string errorMessage)
        {
            errorMessage = default;
            if (!this._client.CanMoveToBottom(selectedIndex)) return false;
            if (selectedIndex < 0 || selectedIndex >= this.Mods.Count - 1)
            {
                errorMessage = "Index out of bounds";
                return false;
            }

            this.Mods.Move(selectedIndex, this.Mods.Count - 1);
            this.Validate();
            return true;
        }

        public bool CheckAll(out string errorMessage)
        {
            errorMessage = default;
            if (!this._client.CanCheckAll()) return false;
            this._runValidation = false;
            foreach (var modEntry in this.Mods.Where(x => x.IsCheckboxEnabled)) modEntry.IsChecked = true;
            this._runValidation = true;
            this.Validate();
            return true;
        }

        public bool UncheckAll(out string errorMessage)
        {
            errorMessage = default;
            if (!this._client.CanUncheckAll()) return false;
            this._runValidation = false;
            foreach (var modEntry in this.Mods.Where(x => x.IsCheckboxEnabled)) modEntry.IsChecked = false;
            this._runValidation = true;
            this.Validate();
            return true;
        }

        public bool InvertCheck(out string errorMessage)
        {
            errorMessage = default;
            if (!this._client.CanInvertCheck()) return false;
            this._runValidation = false;
            foreach (var modEntry in this.Mods.Where(x => x.IsCheckboxEnabled))
                modEntry.IsChecked = !modEntry.IsChecked;
            this._runValidation = true;
            this.Validate();
            return true;
        }

        public bool Sort(out string errorMessage)
        {
            errorMessage = default;
            if (!this._client.CanSort()) return false;
            this._runValidation = false;

            try
            {
                var mods = this.Mods.ToArray();
                this.Mods.Clear();
                var sorted2 = mods.StableOrderTopologicallyBy(x => x.DependsOn);
                foreach (var mod in sorted2) this.Mods.Add(mod);
            }
            catch (Exception e)
            {
                this.Log().Error(e, "TopologicalSort");
                errorMessage = e.Message;
                this._runValidation = true;
                this.Validate();
                return false;
            }

            this._runValidation = true;
            this.Validate();
            return false;
        }

        private string EnabledMods()
        {
            return "_MODULES_" + string.Join(
                "",
                this.Mods.Where(x => x.UserModData.IsSelected).Select(x => "*" + x.Module.Id)) + "*_MODULES_";
        }

        private string GameArguments()
        {
            return $"/singleplayer {this.EnabledMods()}";
        }

        private void BackupFile(string file)
        {
            var path = Path.Combine(this._basePath, "BannerLordLauncher Backups");
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                this.Log().Error(e);
                return;
            }

            if (!File.Exists(file)) return;
            var ext = Path.GetExtension(file);
            var i = 0;
            var newFile = Path.ChangeExtension(file, $"{ext}.{i:D3}");
            Debug.Assert(newFile != null, nameof(newFile) + " != null");
            newFile = Path.Combine(path, Path.GetFileName(newFile));
            while (File.Exists(newFile))
            {
                i++;
                newFile = Path.ChangeExtension(file, $"{ext}.{i:D3}");
                Debug.Assert(newFile != null, nameof(newFile) + " != null");
                newFile = Path.Combine(path, Path.GetFileName(newFile));
            }

            if (i > 999) return;
            try
            {
                Debug.Assert(file != null, nameof(file) + " != null");
                File.Move(file, newFile);
            }
            catch (Exception e)
            {
                this.Log().Error(e);
            }
        }

        private IEnumerable<string> GetAssemblies(ModEntry module)
        {
            var path = Path.Combine(this._modulePath, module.Module.DirectoryName, "bin", "Win64_Shipping_Client");
            if (!Directory.Exists(path)) yield break;

            foreach (var subModule in module.Module.SubModules)
            {
                foreach (var assembly in subModule.Assemblies ?? Enumerable.Empty<string>())
                {
                    var file = Path.Combine(path, assembly);
                    if (File.Exists(file)) yield return file;
                }

                if (string.IsNullOrEmpty(subModule.DLLName)) continue;
                {
                    var file = Path.Combine(path, subModule.DLLName);
                    if (File.Exists(file)) yield return file;
                }
            }

            foreach (var subModule in module.Module.DelayedSubModules)
            {
                foreach (var assembly in subModule.Assemblies ?? Enumerable.Empty<string>())
                {
                    var file = Path.Combine(path, assembly);
                    if (File.Exists(file)) yield return file;
                }

                if (string.IsNullOrEmpty(subModule.DLLName)) continue;
                {
                    var file = Path.Combine(path, subModule.DLLName);
                    if (File.Exists(file)) yield return file;
                }
            }
        }

        private IEnumerable<string> GetAssemblies()
        {
            foreach (var module in this.Mods.Where(x => x.IsChecked))
            {
                foreach (var a in this.GetAssemblies(module))
                    yield return a;
            }
        }

        private void Validate()
        {
            if (!this._runValidation) return;
            lock (this._lock)
            {
                foreach (var entry in this.Mods)
                {
                    if (entry.LoadOrderConflicts == null)
                        entry.LoadOrderConflicts = new ObservableCollection<LoadOrderConflict>();
                    entry.LoadOrderConflicts.Clear();
                }

                for (var i = 0; i < this.Mods.Count; i++)
                {
                    var modEntry = this.Mods[i];
                    foreach (var dependsOn in modEntry.Module.DependedModules)
                    {
                        var found = this.Mods.FirstOrDefault(x => x.Module.Id == dependsOn && x.UserModData.IsSelected);
                        var isDown = false;
                        var isMissing = found == null;
                        if (found != null)
                        {
                            var foundIdx = this.Mods.IndexOf(found);
                            if (foundIdx > i)
                            {
                                isDown = true;
                                found.LoadOrderConflicts.Add(
                                    new LoadOrderConflict
                                        {IsUp = true, DependsOn = modEntry.Module.Id, Optional = false});
                            }
                        }

                        if (!isDown && !isMissing) continue;
                        var conflict = new LoadOrderConflict
                        {
                            IsUp = false,
                            IsDown = isDown,
                            IsMissing = isMissing,
                            DependsOn = dependsOn,
                            Optional = false
                        };
                        modEntry.LoadOrderConflicts.Add(conflict);
                    }

                    foreach (var dependsOn in modEntry.Module.OptionalDependModules)
                    {
                        var found = this.Mods.FirstOrDefault(x => x.Module.Id == dependsOn && x.UserModData.IsSelected);
                        var isDown = false;
                        var isMissing = found == null;
                        if (found != null)
                        {
                            var foundIdx = this.Mods.IndexOf(found);
                            if (foundIdx > i)
                            {
                                isDown = true;
                                found.LoadOrderConflicts.Add(
                                    new LoadOrderConflict
                                        {IsUp = true, DependsOn = modEntry.Module.Id, Optional = true});
                            }
                        }

                        if (!isDown && !isMissing) continue;
                        var conflict = new LoadOrderConflict
                        {
                            IsUp = false,
                            IsDown = isDown,
                            IsMissing = isMissing,
                            DependsOn = dependsOn,
                            Optional = true
                        };
                        modEntry.LoadOrderConflicts.Add(conflict);
                    }
                }

                foreach (var entry in this.Mods)
                {
                    entry.NotifyChanged("HasConflicts");
                    entry.NotifyChanged("Conflicts");
                }
            }
        }

        private void BuildDependencies()
        {
            foreach (var module in this.Mods)
            {
                foreach (var moduleId in module.Module.DependedModules)
                {
                    var found = this.Mods.FirstOrDefault(
                        x => x.Module.Id.Equals(moduleId, StringComparison.OrdinalIgnoreCase));
                    if (found == null) continue;
                    module.DependsOn.Add(found);
                }

                foreach (var assembly in module.DependOnAssemblies)
                {
                    var found = this.Mods.FirstOrDefault(x => x.MyAssemblies.Contains(assembly));
                    if (found == null) continue;
                    if (found == module) continue;
                    module.DependsOn.Add(found);
                }

                foreach (var moduleId in module.Module.OptionalDependModules)
                {
                    var found = this.Mods.FirstOrDefault(
                        x => x.Module.Id.Equals(moduleId, StringComparison.OrdinalIgnoreCase));
                    if (found == null) continue;
                    module.DependsOn.Add(found);
                }
            }
        }

        private void AnalyzeAssemblies()
        {
            foreach (var module in this.Mods)
            {
                module.MyAssemblies.Clear();
                foreach (var assemblyFile in this.GetAssemblies(module))
                    try
                    {
                        var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyFile);
                        module.MyAssemblies.Add(assemblyDefinition.Name.FullName);
                        foreach (var d in assemblyDefinition.MainModule.AssemblyReferences)
                            module.DependOnAssemblies.Add(d.FullName);
                    }
                    catch (Exception e)
                    {
                        this.Log().Error(e);
                    }
            }
        }
    }
}