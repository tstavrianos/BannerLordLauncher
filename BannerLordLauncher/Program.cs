using System;

namespace BannerLordLauncher
{
    using System.Diagnostics;
    using System.IO;

    using Newtonsoft.Json;

    using Sentry;

    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var configurationFilePath = Path.Combine(GetApplicationRoot(), "configuration.json");
            AppConfig config = null;
            try
            {
                if (File.Exists(configurationFilePath))
                {
                    config =
                        JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(configurationFilePath));
                }
            }
            catch
            {
                config = null;
            }

            var submit = true;
            if (config?.Version != null)
            {
                submit = config.SubmitCrashLogs;
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
