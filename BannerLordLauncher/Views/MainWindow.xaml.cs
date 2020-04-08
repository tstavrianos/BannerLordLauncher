using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using BannerLordLauncher.ViewModels;
using Newtonsoft.Json;

namespace BannerLordLauncher.Views
{
    public sealed class MainWindow : Window
    {
        internal AppConfig Configuration { get; }
        public MainWindowViewModel ViewModel => this.DataContext as MainWindowViewModel;

        public MainWindow()
        {
            try
            {
                if (File.Exists("configuration.json"))
                {
                    this.Configuration =
                        JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText("configuration.json"));
                }
            }
            catch
            {
                this.Configuration = null;
            }
            if(this.Configuration == null) this.Configuration = new AppConfig {AutoBackup = true};
            
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            var model = new MainWindowViewModel(this);
            this.DataContext = model;
            model.Initialize();
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
            var settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented};
            File.WriteAllText("configuration.json", JsonConvert.SerializeObject(this.Configuration, settings));

            base.OnClosed(e);
        }
    }
}