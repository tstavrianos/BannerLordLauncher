using System.Windows;
using Serilog;
using Serilog.Exceptions;
using Splat;
using Splat.Serilog;

namespace BannerLordLauncher
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            InitializeLogging();

            if (Program.Configuration?.Version != null && Program.Configuration.SubmitCrashLogs == false)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
                Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            }

            base.OnStartup(e);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "");
            e.SetObserved();
            this.Shutdown();
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "");
            e.Handled = true;
            this.Shutdown();
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception, "");
            e.Handled = true;
            this.Shutdown();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception x)
                Log.Fatal(x, "");
            if (!e.IsTerminating) this.Shutdown();
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
