using System;

namespace Controller {
    class Time {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }

        public Time() {
            DateTime time = Environment.OSVersion.Platform == PlatformID.Unix ? DateTime.Now + TimeSpan.FromHours(1) : DateTime.Now;
            this.Hour = time.Hour;
            this.Minute = time.Minute;
            this.Second = time.Second;
        }

        public void Update() {
            DateTime time = Environment.OSVersion.Platform == PlatformID.Unix ? DateTime.Now + TimeSpan.FromHours(1) : DateTime.Now;
            this.Hour = time.Hour;
            this.Minute = time.Minute;
            this.Second = time.Second;
        }
    }
}
