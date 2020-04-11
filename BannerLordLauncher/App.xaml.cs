using System.Windows;
using Serilog;
using Serilog.Exceptions;
using Splat;
using Splat.Serilog;

namespace BannerLordLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Overrides of Application

        protected override void OnStartup(StartupEventArgs e)
        {
            InitializeLogging();
            base.OnStartup(e);
        }

        #endregion

        private static void InitializeLogging()
        {
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug() //
                .Enrich.WithExceptionDetails() //
#else
                .MinimumLevel.Warning()//
#endif
                .Enrich.FromLogContext() //
                .WriteTo.File("app.log") //
                .CreateLogger();
            Locator.CurrentMutable.UseSerilogFullLogger();
        }
    }
}
