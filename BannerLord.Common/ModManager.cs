using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BannerLord.Common.Xml;
using Splat;

namespace BannerLord.Common
{
    public sealed class ModManager : IEnableLogger
    {
        public ObservableCollection<ModEntry> Mods { get; } = new ObservableCollection<ModEntry>();
        private string _basePath;
        public bool AutomaticValidation { get; set; }
        private bool _runValidation;
        private string _gameExe;
        public bool AutoBackup { get; set; }

        public void Initialize(string game)
        {
            this._runValidation = false;
            this._basePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Mount and Blade II Bannerlord\\Configs\\";
            var launcherData = UserData.Load(this, Path.Combine(this._basePath, "LauncherData.xml")) ?? new UserData();
            this._gameExe = Path.Combine(game, "bin", "Win64_Shipping_Client", "Bannerlord.exe");
            var modulesFolder = Path.Combine(game, "Modules");
            var modules = Directory.EnumerateDirectories(modulesFolder, "*", SearchOption.TopDirectoryOnly).Select(dir => Module.Load(this, Path.GetFileName(dir), game)).Where(module => module != null).ToList();

            if (launcherData.SingleplayerData?.ModDatas != null)
            {
                foreach (var mod in launcherData.SingleplayerData.ModDatas)
                {
                    var module = modules.FirstOrDefault(x => x.Id == mod.Id);
                    if (module == null) continue;
                    modules.Remove(module);
                    var modEntry = new ModEntry {Module = module, UserModData = mod};
                    this.Mods.Add(modEntry);
                    if (modEntry.Module?.Official == true) modEntry.IsChecked = true;
                }
            }

            foreach (var module in modules)
            {
                var modEntry = new ModEntry {Module = module, UserModData = new UserModData(module.Id, false)};
                this.Mods.Add(modEntry);
                if (modEntry.Module?.Official == true) modEntry.IsChecked = true;
            }
            
            this._runValidation = true;
            this.Validate();
        }
        
        private string EnabledMods() => "_MODULES_" + string.Join("", this.Mods.Where(x => x.UserModData.IsSelected).Select(x => "*" + x.Module.Id)) + "*_MODULES_";
        
        private string GameArguments() => $"/singleplayer {this.EnabledMods()}";

        private static void BackupFile(string file)
        {
            if (!File.Exists(file)) return;
            var ext = Path.GetExtension(file);
            var i = 0;
            var newFile = Path.ChangeExtension(file, ext + $".{i:D3}");
            while (File.Exists(newFile))
            {
                i++;
                newFile = Path.ChangeExtension(file, ext + $".{i:D3}");
            }

            if (i <= 999)
            {
                File.Move(file, newFile);
            }
        }

        public void RunGame()
        {
            if (string.IsNullOrEmpty(this._gameExe)) return;
            this.Save();
            var info = new ProcessStartInfo
            {
                Arguments = this.GameArguments(),
                FileName = this._gameExe,
                WorkingDirectory = Path.GetDirectoryName(this._gameExe),
                UseShellExecute = false
            };
            Process.Start(info);
            Thread.Sleep(TimeSpan.FromSeconds(5));
            Environment.Exit(0);
        }
        public void Save()
        {
            var launcherDataFile = Path.Combine(this._basePath, "LauncherData.xml");
            var launcherData = UserData.Load(this, launcherDataFile) ?? new UserData();
            if(launcherData.SingleplayerData == null) launcherData.SingleplayerData = new UserGameTypeData();
            launcherData.SingleplayerData.ModDatas.Clear();
            foreach (var mod in this.Mods)
            {
                launcherData.SingleplayerData.ModDatas.Add(mod.UserModData);
            }
            if(this.AutoBackup) BackupFile(launcherDataFile);
            launcherData.Save(launcherDataFile);
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
            if (this.AutomaticValidation) this.Validate();
        }

        public void ReverseOrder()
        {
            this._runValidation = false;
            for (var i = 0; i < this.Mods.Count; i++)
                this.Mods.Move(this.Mods.Count - 1, i);
            this._runValidation = true;
            if (this.AutomaticValidation) this.Validate();
        }

        public void MoveToTop(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Mods.Count <= selectedIndex) return;
            this.Mods.Move(selectedIndex, 0);
            if (this.AutomaticValidation) this.Validate();
        }

        public void MoveUp(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Mods.Count <= selectedIndex) return;
            this.Mods.Move(selectedIndex, selectedIndex - 1);
            if (this.AutomaticValidation) this.Validate();
        }

        public void MoveDown(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Mods.Count <= selectedIndex + 1) return;
            this.Mods.Move(selectedIndex, selectedIndex + 1);
            if (this.AutomaticValidation) this.Validate();
        }

        public void MoveToBottom(int selectedIndex)
        {
            if (selectedIndex <= 0 || this.Mods.Count <= selectedIndex + 1) return;
            this.Mods.Move(selectedIndex, this.Mods.Count - 1);
            if (this.AutomaticValidation) this.Validate();
        }

        public void CheckAll()
        {
            this._runValidation = false;
            foreach (var modEntry in this.Mods.Where(x => x.IsCheckboxEnabled))
            {
                modEntry.IsChecked = true;
            }
            this._runValidation = true;
            if (this.AutomaticValidation) this.Validate();
        }

        public void UncheckAll()
        {
            this._runValidation = false;
            foreach (var modEntry in this.Mods.Where(x => x.IsCheckboxEnabled))
            {
                modEntry.IsChecked = false;
            }
            this._runValidation = true;
            if (this.AutomaticValidation) this.Validate();
        }

        public void InvertCheck()
        {
            this._runValidation = false;
            foreach (var modEntry in this.Mods.Where(x => x.IsCheckboxEnabled))
            {
                modEntry.IsChecked = !modEntry.IsChecked;
            }
            this._runValidation = true;
            if (this.AutomaticValidation) this.Validate();
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
                if(entry.LoadOrderConflicts == null) entry.LoadOrderConflicts = new ObservableCollection<LoadOrderConflict>();
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
                            found.LoadOrderConflicts.Add(new LoadOrderConflict { IsUp = true, DependsOnId = modEntry.Module.Id, DependsOnName = modEntry.DisplayName });
                        }
                    }

                    if (!isDown && !isMissing) continue;
                    var conflict = new LoadOrderConflict
                    {
                        IsUp = false,
                        IsDown = isDown,
                        IsMissing = isMissing,
                        DependsOnId = dependsOn,
                        DependsOnName = found?.DisplayName
                    };
                    modEntry.LoadOrderConflicts.Add(conflict);
                }
            }
        }
    }
}