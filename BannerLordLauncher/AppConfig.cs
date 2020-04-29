using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        
        [JsonProperty("sorting")]
        public List<Sorting> Sorting { get; set; }

        public void CopyFrom(AppConfig other)
        {
            this.Version = other.Version;
            this.GamePath = other.GamePath;
            this.ConfigPath = other.ConfigPath;
            this.CheckForUpdates = other.CheckForUpdates;
            this.CloseWhenRunningGame = other.CloseWhenRunningGame;
            this.SubmitCrashLogs = other.SubmitCrashLogs;
            this.WarnOnConflict = other.WarnOnConflict;
            this.ExtraGameArguments = other.ExtraGameArguments;
            this.GameExeId = other.GameExeId;
            
            if(this.Sorting == null) this.Sorting = new List<Sorting>();
            this.Sorting.Clear();
            if (other.Sorting == null) return;
            foreach (var s in other.Sorting)
            {
                this.Sorting.Add(s.Clone());
            }
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SortType
    {
        [EnumMember(Value = "id")]
        Id = 0,
        [EnumMember(Value = "name")]
        Name = 1,
        [EnumMember(Value = "version")]
        Version = 2,
        [EnumMember(Value = "official")]
        Official = 3,
        [EnumMember(Value = "native")]
        Native = 4,
        [EnumMember(Value = "selected")]
        Selected = 5
    }

    public class Sorting
    {
        [JsonProperty("type")]
        public SortType Type { get; set; }
        [JsonProperty("ascending")]
        public bool Ascending { get; set; }

        public Sorting Clone() => new Sorting{Type = this.Type, Ascending = this.Ascending};
    }
}