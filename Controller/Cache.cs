using System;

namespace Controller {
    class Cache {
        public DateTime LastCacheUpdate { get; set; }
        public TimeSpan UpdateTimeSpan { get; set; } = TimeSpan.FromHours(1);
        public Func<byte[]> CacheUpdater { get; set; }

        private byte[] cache;

        public byte[] Value {
            get {
                if((DateTime.Now - this.LastCacheUpdate) > this.UpdateTimeSpan || this.LastCacheUpdate == null) {
                    cache = this.CacheUpdater.Invoke();
                    this.LastCacheUpdate = DateTime.Now;
                }
                return cache;
            }
            set => cache = value;
        }
    }
}
