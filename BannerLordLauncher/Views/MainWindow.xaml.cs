using Alphaleonis.Win32.Filesystem;
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
        public MainWindow()
        {
            this.InitializeComponent();

            var model = new MainWindowViewModel(this);
            this.DataContext = model;
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            if (Program.Configuration?.Version == null)
            {
                var o = new OptionsDialog();
                o.Owner = this;
                o.ShowDialog();
                if (o.Result)
                {
                    Program.Configuration = o.Config;
                }
                else
                {
                    Environment.Exit(0);
                }
            }
            (this.DataContext as MainWindowViewModel).Initialize();
            this.SetPlacement(Program.Configuration.Placement);
            this.Activated -= this.OnActivated;
        }

        private static string GetApplicationRoot()
        {
            return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Program.Configuration.Placement = this.GetPlacement();
            var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
            try
            {
                File.WriteAllText(
                    Program.ConfigurationFilePath,
                    JsonConvert.SerializeObject(Program.Configuration, settings));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error writing the configuration to disk");
            }
        }

        private void ButtonBase_OnClickCog(object sender, RoutedEventArgs e)
        {
            var w = new OptionsDialog();
            w.Owner = this;
            w.ShowDialog();
            if (!w.Result) return;
            Program.Configuration.CopyFrom(w.Config);
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
