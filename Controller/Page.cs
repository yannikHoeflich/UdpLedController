using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Controller {
    internal class Page {
        public Regex Regex { get; set; }
        public Func<InputData, OutputData> Method { get; set; }
        public bool CanCache { get; set; } = true;

        private Cache cache;

        public Cache Cache {
            get => this.CanCache ? cache : throw new AccessViolationException("the page is not allowed to have a cache");
            set => cache = this.CanCache ? value : throw new AccessViolationException("the page is not allowed to have a cache");
        }

        private List<Cache> extraCache;

        public List<Cache> ExtraCache {
            get => this.CanCache ? extraCache : throw new AccessViolationException("the page is not allowed to have a cache");
            set => extraCache = this.CanCache ? value : throw new AccessViolationException("the page is not allowed to have a cache");
        }
    }
}
