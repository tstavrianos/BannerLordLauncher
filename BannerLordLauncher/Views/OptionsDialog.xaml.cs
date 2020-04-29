using System;
using System.Windows;
using ReactiveUI;

namespace BannerLordLauncher.Views
{
    using System.Diagnostics;
    using System.Linq;
    using Alphaleonis.Win32.Filesystem;
    using System.Windows.Threading;
    using System.Collections.Generic;

    using BannerLordLauncher.Controls.MessageBox;

    using Ookii.Dialogs.Wpf;

    using Steam.Common;
    using System.Collections.ObjectModel;

    public class SortingModel: ReactiveObject
    {
        private bool _ascending;

        public bool Ascending
        {
            get => this._ascending;
            set
            {
                this.RaiseAndSetIfChanged(ref this._ascending, value);
                this.RaisePropertyChanged("AscendingName");
                this.RaisePropertyChanged("Tip");
            }
        }
        
        public SortingModel() {}

        public string AscendingName => this.Ascending ? "Ascending" : "Descending";

        public SortType Type { get; set; }
        public string Name {
            get
            {
                switch (this.Type)
                {
                    case SortType.Id: return "Id";
                    case SortType.Name: return "Name";
                    case SortType.Version: return "Version";
                    case SortType.Official: return "Official";
                    case SortType.Native: return "Native";
                    case SortType.Selected: return "Selected";
                }
                return null;
            }
        }

        public string Tip
        {
            get
            {
                switch (this.Type)
                {
                    case SortType.Id: return $"Alphabetical by Id, {this.AscendingName}";
                    case SortType.Name: return $"Alphabetical by Id, {this.AscendingName}";
                    case SortType.Version: return $"By Version, {this.AscendingName}";
                    case SortType.Official: return "Official mods, " + (this.Ascending ? "first" : "last");
                    case SortType.Native: return "Native mod, " + (this.Ascending ? "first" : "last");
                    case SortType.Selected: return "Selected mods, " + (this.Ascending ? "first" : "last");
                }

                return string.Empty;
            }
        }
        
        public override string ToString()
        {
            return this.Name;
        }
    }
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        public AppConfig Config { get; }

        public bool Result { get; private set; }
        public ObservableCollection<SortingModel> Sorting {get;}
        public ObservableCollection<SortingModel> Sorting2 {get;}

        public OptionsDialog()
        {
            this.Config = new AppConfig();
            if (Program.Configuration != null)
                this.Config.CopyFrom(Program.Configuration);
            if (this.Config?.Version == null)
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var config = string.Empty;
                if (Directory.Exists(basePath))
                {
                    basePath = Path.Combine(basePath, "Mount and Blade II Bannerlord");
                    if (Directory.Exists(basePath))
                    {
                        basePath = Path.Combine(basePath, "Configs");
                        if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
                        config = basePath;
                    }
                }

                var game = string.Empty;
                var steamFinder = new SteamFinder();
                if (steamFinder.FindSteam())
                {
                    game = steamFinder.FindGameFolder(261550);
                    if (string.IsNullOrEmpty(game) || !Directory.Exists(game))
                    {
                        game = null;
                    }
                }
                this.Config = new AppConfig
                {
                    Version = 1,
                    Placement = new WindowPlacement.Data
                    {
                        normalPosition = new WindowPlacement.Rect(0, 0, 604, 730)
                    },
                    CheckForUpdates = true,
                    CloseWhenRunningGame = true,
                    SubmitCrashLogs = true,
                    WarnOnConflict = true,
                    ConfigPath = config,
                    GamePath = game,
                    Sorting = new List<Sorting>()
                };
            }
            this.InitializeComponent();
            this.DataContext = this;
            this.Result = false;
            this.Sorting = new ObservableCollection<SortingModel>();
            this.Sorting2 = new ObservableCollection<SortingModel>();
            this.Sorting.Add(new SortingModel{Type = SortType.Id, Ascending = true});
            this.Sorting.Add(new SortingModel{Type = SortType.Name, Ascending = true});
            this.Sorting.Add(new SortingModel{Type = SortType.Native, Ascending = true});
            this.Sorting.Add(new SortingModel{Type = SortType.Official, Ascending = true});
            this.Sorting.Add(new SortingModel{Type = SortType.Version, Ascending = true});
            this.Sorting.Add(new SortingModel{Type = SortType.Selected, Ascending = true});

            if(this.Config.Sorting != null) {
                foreach(var s in this.Config.Sorting)
                {
                    var found = this.Sorting.FirstOrDefault(x => x.Type == s.Type);
                    if(found == null) continue;
                    this.Sorting.Remove(found);
                    this.Sorting2.Add(new SortingModel{Type = s.Type, Ascending = s.Ascending});
                }
            }
        }

        private static string GetApplicationRoot()
        {
            return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        }

        private void ButtonBase_OnClickCancel(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
        }

        private void ButtonBase_OnClickOk(object sender, RoutedEventArgs e)
        {
            if (!ValidConfigFolder(this.Config.ConfigPath))
            {
                var r = this.FindConfigFolder();
                if (r == null) return;
                this.Config.ConfigPath = r;
            }
            if (!ValidGameFolder(this.Config.GamePath))
            {
                var r = this.FindGameFolder();
                if (r == null) return;
                this.Config.GamePath = r;
            }

            if (!ValidConfigFolder(this.Config.ConfigPath) || !ValidGameFolder(this.Config.GamePath)) return;

            this.Config.Sorting.Clear();
            foreach (var sorting in this.Sorting2)
            {
                this.Config.Sorting.Add(new Sorting{Type = sorting.Type, Ascending = sorting.Ascending});
            }
            this.Result = true;
            this.Close();
        }

        private void SafeMessage(string message)
        {
            Debug.Assert(Application.Current.Dispatcher != null, "Application.Current.Dispatcher != null");
            if (Application.Current.Dispatcher.CheckAccess())
            {
                MyMessageBox.Show(this, message);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => MyMessageBox.Show(this, message)));
            }
        }

        private string FindGameFolder()
        {
            while (true)
            {
                var dialog = new VistaFolderBrowserDialog
                {
                    Description = "Select game root folder",
                    UseDescriptionForTitle = true,
                    SelectedPath = this.Config.GamePath
                };
                var result = dialog.ShowDialog(this);
                if (result is null) return null;
                if (!ValidGameFolder(dialog.SelectedPath)) continue;
                return dialog.SelectedPath;
            }
        }

        private static bool ValidGameFolder(string selectedPath)
        {
            return (Directory.Exists(selectedPath) && File.Exists(Path.Combine(selectedPath, "bin", "Win64_Shipping_Client", "Bannerlord.exe")));
        }

        private static bool ValidConfigFolder(string selectedPath)
        {
            return Directory.Exists(selectedPath);

        }

        private string FindConfigFolder()
        {
            while (true)
            {
                var dialog = new VistaFolderBrowserDialog
                {
                    Description = "Select game config folder, in documents",
                    UseDescriptionForTitle = true,
                    SelectedPath = this.Config.ConfigPath
                };
                var result = dialog.ShowDialog(this);
                if (result is null) return null;
                if (!ValidConfigFolder(dialog.SelectedPath)) continue;
                return dialog.SelectedPath;
            }
        }

        private void ButtonBase_OnClickGame(object sender, RoutedEventArgs e)
        {
            var r = this.FindGameFolder();
            if (r == null) return;
            this.Config.GamePath = r;
        }

        private void ButtonBase_OnClickConfig(object sender, RoutedEventArgs e)
        {
            var r = this.FindConfigFolder();
            if (r == null) return;
            this.Config.ConfigPath = r;
        }
    }
}
