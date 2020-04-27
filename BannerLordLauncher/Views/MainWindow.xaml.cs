using System.IO;
using BannerLordLauncher.ViewModels;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Windows;

namespace BannerLordLauncher.Views
{
    using Serilog;

    public partial class MainWindow
    {
        internal AppConfig Configuration { get; }
        private readonly string _configurationFilePath;

        public MainWindow()
        {
            this._configurationFilePath = Path.Combine(GetApplicationRoot(), "configuration.json");
            try
            {
                if (File.Exists(this._configurationFilePath))
                {
                    this.Configuration =
                        JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(this._configurationFilePath));
                }
            }
            catch
            {
                this.Configuration = null;
            }

            if (this.Configuration?.Version == null)
            {
                var o = new OptionsDialog();
                o.ShowDialog();
                if (o.Result)
                {
                    this.Configuration = o.Config;
                }
                else
                {
                    Environment.Exit(0);
                }
            }

            this.InitializeComponent();

            var model = new MainWindowViewModel(this);
            this.DataContext = model;
            model.Initialize();
        }

        private static string GetApplicationRoot()
        {
            return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.SetPlacement(this.Configuration.Placement);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            this.Configuration.Placement = this.GetPlacement();
            var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
            try
            {
                File.WriteAllText(
                    this._configurationFilePath,
                    JsonConvert.SerializeObject(this.Configuration, settings));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error writing the configuration to disk");
            }
        }

        private void ButtonBase_OnClickCog(object sender, RoutedEventArgs e)
        {
            var w = new OptionsDialog();
            w.ShowDialog();
            if (!w.Result) return;
            this.Configuration.GamePath = w.Config.GamePath;
            this.Configuration.ConfigPath = w.Config.ConfigPath;
            this.Configuration.CheckForUpdates = w.Config.CheckForUpdates;
            this.Configuration.CloseWhenRunningGame = w.Config.CloseWhenRunningGame;
            this.Configuration.SubmitCrashLogs = w.Config.SubmitCrashLogs;
            this.Configuration.WarnOnConflict = w.Config.WarnOnConflict;
            this.Configuration.ExtraGameArguments = w.Config.ExtraGameArguments;
            this.Configuration.GameExeId = w.Config.GameExeId;
        }

        private void RefreshMaximizeRestoreButton()
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                this.maximizeButton.Visibility = System.Windows.Visibility.Collapsed;
                this.restoreButton.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.maximizeButton.Visibility = System.Windows.Visibility.Visible;
                this.restoreButton.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            this.RefreshMaximizeRestoreButton();
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
