using System;

namespace BannerLordLauncher
{
    using Sentry;

    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            using (SentrySdk.Init("http://fb8a882e37f14a13be3a802570dfd640@d2allgr.duckdns.org:9001/2"))
            {
                App.Main();
            }
        }
    }
}
