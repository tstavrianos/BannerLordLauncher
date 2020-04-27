using System;
using System.Windows;

namespace BannerLordLauncher.Views
{
    using System.Diagnostics;
    using Alphaleonis.Win32.Filesystem;
    using System.Windows.Threading;

    using BannerLordLauncher.Controls.MessageBox;

    using Newtonsoft.Json;

    using Ookii.Dialogs.Wpf;

    using Serilog;

    using Steam.Common;

    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        public AppConfig Config { get; }

        public bool Result { get; private set; }
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
                    GamePath = game
                };
            }
            this.InitializeComponent();
            this.DataContext = this;
            this.Result = false;
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
