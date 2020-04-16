using Newtonsoft.Json;

namespace BannerLordLauncher
{
    public class AppConfig
    {
        [JsonProperty("gamePath")]
        public string GamePath { get; set; }

        [JsonProperty("placement")]
        public WindowPlacement.Data Placement { get; set; }

        [JsonProperty("configPath")]
        public string ConfigPath { get; set; }
    }
}