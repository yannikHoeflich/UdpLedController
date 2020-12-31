
using Newtonsoft.Json;

namespace Led_Controller {
    internal class Animation {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("isAnimated")]
        public bool IsAnimated { get; set; }

        public static bool operator ==( Animation obj1, Animation obj2 ) => obj1.Equals(obj2);
        public static bool operator !=( Animation obj1, Animation obj2 ) => !obj1.Equals(obj2);

        public override bool Equals( object o ) => o is Animation animation &&
                                                        animation.Name == this.Name &&
                                                        animation.IsAnimated == this.IsAnimated;
        public override int GetHashCode() => this.Name.GetHashCode();

        public override string ToString() => this.Name;
    }
}
