using Newtonsoft.Json;

namespace BannerLordLauncher
{
    public class AppConfig
    {
        [JsonProperty("gamePath")]
        public string GamePath { get; set; }
        
        [JsonProperty("autoValidate")]
        public bool AutoValidate { get; set; }
        
        [JsonProperty("autoBackup")]
        public bool AutoBackup { get; set; }
    }
}