using System.Collections.Generic;

using Newtonsoft.Json;

namespace Controller {
    internal class Animation {
        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("scriptPath")]
        public string ScriptPath { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isAnimated")]
        public bool IsAnimated { get; set; }

        [JsonProperty("delay")]
        public int Delay { get; set; }

        [JsonProperty("data")]
        public Dictionary<string, dynamic> Data { get; set; } = null;

        [JsonProperty("hidden")]
        public bool Hidden { get; set; }

        public override bool Equals( object o ) {
            return o != null &&
                    o is Animation animation &&
                    animation.ScriptPath == this.ScriptPath &&
                    animation.Name == this.Name &&
                    animation.IsAnimated == this.IsAnimated &&
                    animation.Delay == this.Delay &&
                    animation.DownloadUrl == this.DownloadUrl;
        }
        public override int GetHashCode() => this.Name.GetHashCode() + this.Data.GetHashCode();

        public override string ToString() => this.Name + ": " + JsonConvert.SerializeObject(this.Data)[1..^1];
    }
}
