using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using BannerLordLauncher.Avalonia.ViewModels;
using Newtonsoft.Json;

namespace BannerLordLauncher.Avalonia.Views
{
    public sealed class MainWindow : Window
    {
        internal AppConfig Configuration { get; }
        public MainWindowViewModel ViewModel => this.DataContext as MainWindowViewModel;
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
            if (this.Configuration == null) this.Configuration = new AppConfig();

            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            var model = new MainWindowViewModel(this);
            this.DataContext = model;
            model.Initialize();
        }

        private static string GetApplicationRoot()
        {
            return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        }

        public void StartDrag(object sender, PointerPressedEventArgs e) => this.ViewModel.StartDrag(sender, e);

        public void DoDrag(object sender, PointerEventArgs e) => this.ViewModel.DoDrag(sender, e);

        public void EndDrag(object sender, PointerReleasedEventArgs e) => this.ViewModel.EndDrag(sender, e);

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
            File.WriteAllText(this._configurationFilePath, JsonConvert.SerializeObject(this.Configuration, settings));

            base.OnClosed(e);
        }
    }
}