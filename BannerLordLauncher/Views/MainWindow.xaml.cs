using System.IO;
using BannerLordLauncher.ViewModels;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace BannerLordLauncher.Views
{

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

            if (this.Configuration == null)
            {
                this.Configuration = new AppConfig();
                this.Configuration.Placement = new WindowPlacement.Data { normalPosition = new WindowPlacement.Rect(0, 0, 604, 730) };
            }
            InitializeComponent();

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
            File.WriteAllText(this._configurationFilePath, JsonConvert.SerializeObject(this.Configuration, settings));

        }
    }
}
