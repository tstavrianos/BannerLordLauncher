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
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            InitializeLogging();
            base.OnStartup(e);
        }

        private static void InitializeLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithExceptionDetails() //

#if DEBUG
                .MinimumLevel.Debug() //
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
