using Newtonsoft.Json;

namespace BannerLordLauncher
{
    public class AppConfig
    {
        [JsonProperty("version")]
        public int? Version { get; set; }

        [JsonProperty("gamePath")]
        public string GamePath { get; set; }

        [JsonProperty("placement")]
        public WindowPlacement.Data Placement { get; set; }

        [JsonProperty("configPath")]
        public string ConfigPath { get; set; }

        [JsonProperty("checkForUpdates")]
        public bool CheckForUpdates { get; set; }

        [JsonProperty("submitCrashLogs")]
        public bool SubmitCrashLogs { get; set; }

        [JsonProperty("closeWhenRunningGame")]
        public bool CloseWhenRunningGame { get; set; }

        [JsonProperty("warnOnConflict")]
        public bool WarnOnConflict { get; set; }

        [JsonProperty("extraGameArguments")]
        public string ExtraGameArguments { get; set; }

        [JsonProperty("gameExeId")]
        public int GameExeId { get; set; }

        public void CopyFrom(AppConfig other)
        {
            this.GamePath = other.GamePath;
            this.ConfigPath = other.ConfigPath;
            this.CheckForUpdates = other.CheckForUpdates;
            this.CloseWhenRunningGame = other.CloseWhenRunningGame;
            this.SubmitCrashLogs = other.SubmitCrashLogs;
            this.WarnOnConflict = other.WarnOnConflict;
            this.ExtraGameArguments = other.ExtraGameArguments;
            this.GameExeId = other.GameExeId;
        }
    }
}