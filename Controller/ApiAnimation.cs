
using Newtonsoft.Json;

namespace Controller {

    // clearer class for Api responses with fewer data (only that which are needed by clients)
    class ApiAnimation {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isAnimated")]
        public bool IsAnimated { get; set; }

        public ApiAnimation( Animation animation ) {
            this.Name = animation.Name;
            this.IsAnimated = animation.IsAnimated;
        }
    }
}
