using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BannerLord.Common.Xml;
using Splat;
using System.Collections.Generic;

namespace BannerLord.Common
{

    public sealed class ModManager : IEnableLogger
    {
        public ObservableCollection<ModEntry> Mods { get; } = new ObservableCollection<ModEntry>();
        private string _basePath;
        private bool _runValidation;

        private string _modulePath;
        public string GameExe { get; private set; }

        public void Initialize(string config, string game)
        {
            this._runValidation = false;
            this._basePath = config;
            if (!Directory.Exists(this._basePath))
            {
                this.Log().Error($"{this._basePath} does not exist");
                return;
            }
            if (!Directory.Exists(Path.Combine(this._basePath, "BannerLordLauncher Backups"))) Directory.CreateDirectory(Path.Combine(this._basePath, "BannerLordLauncher Backups"));
            var launcherData = UserData.Load(this, Path.Combine(this._basePath, "LauncherData.xml")) ?? new UserData();
            this._modulePath = Path.Combine(game, "Modules");
            this.GameExe = Path.Combine(game, "bin", "Win64_Shipping_Client", "Bannerlord.exe");
            var modulesFolder = Path.Combine(game, "Modules");
            if (!Directory.Exists(modulesFolder))
            {
                this.Log().Error($"{modulesFolder} does not exist");
                return;
            }
            var modules = Directory.EnumerateDirectories(modulesFolder, "*", SearchOption.TopDirectoryOnly).Select(dir => Module.Load(this, Path.GetFileName(dir), game)).Where(module => module != null).ToList();

            if (launcherData.SingleplayerData?.ModDatas != null)
            {
                foreach (var mod in launcherData.SingleplayerData.ModDatas)
                {
                    if (this.Mods.Any(x => x.UserModData.Id.Equals(mod.Id, StringComparison.OrdinalIgnoreCase))) continue;
                    var module = modules.FirstOrDefault(x => x.Id == mod.Id);
                    if (module == null)
                    {
                        this.Log().Warn($"{mod.Id} could not be found in {modulesFolder}");
                        continue;
                    }

                    modules.Remove(module);
                    var modEntry = new ModEntry { Module = module, UserModData = mod };
                    this.Mods.Add(modEntry);
                    if (modEntry.Module.Official == true) modEntry.IsChecked = true;
                }
            }

            foreach (var module in modules)
            {
                if (this.Mods.Any(x => x.Module.Id.Equals(module.Id, StringComparison.OrdinalIgnoreCase))) continue;
                var modEntry = new ModEntry { Module = module, UserModData = new UserModData(module.Id, false) };
                this.Mods.Add(modEntry);
                if (modEntry.Module.Official == true) modEntry.IsChecked = true;
            }

            this._runValidation = true;
            this.Validate();
        }

        private string EnabledMods() => "_MODULES_" + string.Join("", this.Mods.Where(x => x.UserModData.IsSelected).Select(x => "*" + x.Module.Id)) + "*_MODULES_";

        private string GameArguments() => $"/singleplayer {this.EnabledMods()}";

        private void BackupFile(string file)
        {
            if (!File.Exists(file)) return;
            if (!Directory.Exists(Path.Combine(this._basePath, "BannerLordLauncher Backups"))) Directory.CreateDirectory(Path.Combine(this._basePath, "BannerLordLauncher Backups"));
            var ext = Path.GetExtension(file);
            var i = 0;
            var newFile = Path.ChangeExtension(file, $"{ext}.{i:D3}");
            newFile = Path.Combine(this._basePath, "BannerLordLauncher Backups", Path.GetFileName(newFile));
            while (File.Exists(newFile))
            {
                i++;
                newFile = Path.ChangeExtension(file, $"{ext}.{i:D3}");
                newFile = Path.Combine(this._basePath, "BannerLordLauncher Backups", Path.GetFileName(newFile));
            }

            if (i <= 999)
            {
                try
                {
                    File.Move(file, newFile);
                }
                catch (Exception e)
                {
                    this.Log().Error(e);
                }
            }
        }

        private IEnumerable<string> GetAssemblies()
        {
            foreach (var module in this.Mods.Where(x => x.IsChecked))
            {
                var path = Path.Combine(this._modulePath, module.Module.DirectoryName, "bin", "Win64_Shipping_Client");
                if (!Directory.Exists(path)) continue;

                foreach (var subModule in module.Module.SubModules)
                {

                    foreach (var assembly in subModule.Assemblies ?? Enumerable.Empty<string>())
                    {
                        var file = Path.Combine(path, assembly);
                        if (File.Exists(file)) yield return file;
                    }

                    if (!string.IsNullOrEmpty(subModule.DLLName))
                    {
                        var file = Path.Combine(path, subModule.DLLName);
                        if (File.Exists(file)) yield return file;
                    }
                }
            }
        }

        public void RunGame()
        {
            if (string.IsNullOrEmpty(this.GameExe))
            {
                this.Log().Error("Game executable could not be detected");
                return;
            }
            if (!File.Exists(this.GameExe))
            {
                this.Log().Error($"{this.GameExe} could not be found");
                return;
            }

            foreach (var dll in this.GetAssemblies().Distinct())
            {
                UnblockFiles.ZoneHelper.Remove(dll, this);
            }
            this.Log().Warn($"Trying to execute: {this.GameExe} {this.GameArguments()}");
            var info = new ProcessStartInfo
            {
                Arguments = this.GameArguments(),
                FileName = this.GameExe,
                WorkingDirectory = Path.GetDirectoryName(this.GameExe),
                UseShellExecute = false
            };
            try
            {
                Process.Start(info);
            }
            catch (Exception e)
            {
                this.Log().Error(e);
                return;
            }

            Thread.Sleep(TimeSpan.FromSeconds(5));
            Environment.Exit(0);
        }

        public void OpenConfig()
        {
            if (string.IsNullOrEmpty(this._basePath) || !Directory.Exists(this._basePath)) return;
            var info = new ProcessStartInfo
            {
                Arguments = this._basePath,
                FileName = "explorer.exe",
                WorkingDirectory = this._basePath,
                UseShellExecute = false
            };
            Process.Start(info);
        }

        public void Save()
        {
            var launcherDataFile = Path.Combine(this._basePath, "LauncherData.xml");
            var launcherData = UserData.Load(this, launcherDataFile) ?? new UserData();
            if (launcherData.SingleplayerData == null) launcherData.SingleplayerData = new UserGameTypeData();
            launcherData.SingleplayerData.ModDatas.Clear();
            foreach (var mod in this.Mods)
            {
                launcherData.SingleplayerData.ModDatas.Add(mod.UserModData);
            }

            this.BackupFile(launcherDataFile);
            try
            {
                launcherData.Save(launcherDataFile);
            }
            catch (Exception e)
            {
                this.Log().Error(e);
            }
        }

        public void AlphaSort()
        {
            this._runValidation = false;
            var items = this.Mods.OrderBy(x => x.DisplayName).ToArray();
            this.Mods.Clear();
            foreach (var item in items)
            {
                this.Mods.Add(item);
            }
            this._runValidation = true;
            this.Validate();
        }

        public void ReverseOrder()
        {
            this._runValidation = false;
            for (var i = 0; i < this.Mods.Count; i++)
                this.Mods.Move(this.Mods.Count - 1, i);
            this._runValidation = true;
            this.Validate();
        }

        public void MoveToTop(int selectedIndex)
        {
            if (selectedIndex <= 0 || selectedIndex >= this.Mods.Count) return;
            this.Mods.Move(selectedIndex, 0);
            this.Validate();
        }

        public void MoveUp(int selectedIndex)
        {
            if (selectedIndex <= 0 || selectedIndex >= this.Mods.Count) return;
            this.Mods.Move(selectedIndex, selectedIndex - 1);
            this.Validate();
        }

        public void MoveDown(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= this.Mods.Count - 1) return;
            this.Mods.Move(selectedIndex, selectedIndex + 1);
            this.Validate();
        }

        public void MoveToBottom(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= this.Mods.Count - 1) return;
            this.Mods.Move(selectedIndex, this.Mods.Count - 1);
            this.Validate();
        }

        public void CheckAll()
        {
            this._runValidation = false;
            foreach (var modEntry in this.Mods.Where(x => x.IsCheckboxEnabled))
            {
                modEntry.IsChecked = true;
            }
            this._runValidation = true;
            this.Validate();
        }

        public void UncheckAll()
        {
            this._runValidation = false;
            foreach (var modEntry in this.Mods.Where(x => x.IsCheckboxEnabled))
            {
                modEntry.IsChecked = false;
            }
            this._runValidation = true;
            this.Validate();
        }

        public void InvertCheck()
        {
            this._runValidation = false;
            foreach (var modEntry in this.Mods.Where(x => x.IsCheckboxEnabled))
            {
                modEntry.IsChecked = !modEntry.IsChecked;
            }
            this._runValidation = true;
            this.Validate();
        }

        public void TopologicalSort()
        {
            this._runValidation = false;
            try
            {
                var sorted1 = this.Mods.TopologicalSort(x => x.Module.DependedModules?.Select(d => this.Mods.FirstOrDefault(y => string.Equals(y.Module.Id, d, StringComparison.OrdinalIgnoreCase))).Where(m => m != null) ?? Enumerable.Empty<ModEntry>()).ToArray();
                this.Mods.Clear();
                foreach (var entry in sorted1) this.Mods.Add(entry);
            }
            catch (Exception e)
            {
                this.Log().Error(e, "TopologicalSort");
            }
            this._runValidation = true;
            this.Validate();
        }

        public void Validate()
        {
            if (!this._runValidation) return;
            foreach (var entry in this.Mods)
            {
                if (entry.LoadOrderConflicts == null) entry.LoadOrderConflicts = new ObservableCollection<LoadOrderConflict>();
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
                            found.LoadOrderConflicts.Add(new LoadOrderConflict { IsUp = true, DependsOn = modEntry.Module.Id });
                        }
                    }

                    if (!isDown && !isMissing) continue;
                    var conflict = new LoadOrderConflict
                    {
                        IsUp = false,
                        IsDown = isDown,
                        IsMissing = isMissing,
                        DependsOn = dependsOn,
                    };
                    modEntry.LoadOrderConflicts.Add(conflict);
                }
            }
        }
    }
}