namespace BannerLordLauncher
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Alphaleonis.Win32.Filesystem;
    using Newtonsoft.Json;
    using Sentry;

    public static class Program
    {
        internal static AppConfig Configuration;

        internal static string ConfigurationFilePath;
        [STAThread]
        public static void Main(string[] args)
        {
            ConfigurationFilePath = Path.Combine(GetApplicationRoot(), "configuration.json");
            Configuration = null;
            try
            {
                if (File.Exists(ConfigurationFilePath))
                {
                    Configuration =
                        JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(ConfigurationFilePath));
                }
            }
            catch
            {
                Configuration = null;
            }

            var submit = true;
            if (Configuration?.Version != null)
            {
                submit = Configuration.SubmitCrashLogs;
            }

            if (submit)
            {
                using (SentrySdk.Init("http://fb8a882e37f14a13be3a802570dfd640@d2allgr.duckdns.org:9001/2"))
                {
                    App.Main();
                }
            }
            else
            {
                App.Main();
            }
        }

        private static string GetApplicationRoot()
        {
            return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        }
    }
}
