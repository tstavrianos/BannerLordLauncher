using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using Serilog;
using Serilog.Exceptions;
using Splat;
using Splat.Serilog;

namespace BannerLordLauncher
{
    internal static class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            InitializeLogging();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI();
        
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
            SerilogLogger.Initialize(Log.Logger);
            Locator.CurrentMutable.UseSerilogFullLogger();
        }
    }
}
