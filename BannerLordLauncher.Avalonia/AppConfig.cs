using Newtonsoft.Json;

namespace BannerLordLauncher.Avalonia
{
    public class AppConfig
    {
        [JsonProperty("gamePath")]
        public string GamePath { get; set; }


        [JsonProperty("configPath")]
        public string ConfigPath { get; set; }
    }
}