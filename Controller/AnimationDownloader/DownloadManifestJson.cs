
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Controller.AnimationDownloader {
    class DownloadManifestJson : Animation {
        [JsonProperty("dependencies")]
        public List<string> Dependencies { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        //converts the inheriting class to normal animation object (formats name and removes dependencies from data)
        public Animation ToAnimation( string scriptPath ) => string.IsNullOrEmpty(this.Name) ? null : new Animation() {
            Data = this.Data,
            Delay = this.Delay,
            IsAnimated = this.IsAnimated,
            Name = this.DownloadUrl.StartsWith("https://raw.githubusercontent.com/yannikHoeflich/LedAnimations/master/") ? this.Name : $"{this.Author}.{this.Name}",
            ScriptPath = scriptPath,
            DownloadUrl = this.DownloadUrl,
            Version = this.Version,
            Hidden = this.Hidden
        };
    }
}
