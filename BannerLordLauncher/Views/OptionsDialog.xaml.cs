using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BannerLordLauncher.Views
{
    using System.Diagnostics;
    using System.IO;
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

        private readonly string _configurationFilePath;

        public bool Result { get; private set; }
        public OptionsDialog()
        {
            this._configurationFilePath = Path.Combine(GetApplicationRoot(), "configuration.json");
            try
            {
                if (File.Exists(this._configurationFilePath))
                {
                    this.Config =
                        JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(this._configurationFilePath));
                }
            }
            catch
            {
                this.Config = null;
            }

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

            var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
            try
            {
                File.WriteAllText(
                    this._configurationFilePath,
                    JsonConvert.SerializeObject(this.Config, settings));
                this.Result = true;
                this.Close();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error writing the configuration to disk");
                this.SafeMessage(ex.Message);
                this.Result = false;
            }
        }

        private static void RunInDispatcher(Action a)
        {
            Debug.Assert(Application.Current.Dispatcher != null, "Application.Current.Dispatcher != null");
            if (Application.Current.Dispatcher.CheckAccess())
            {
                a();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, a);
            }
        }

        private void SafeMessage(string message)
        {
            RunInDispatcher(() => MyMessageBox.Show(this, message));
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
