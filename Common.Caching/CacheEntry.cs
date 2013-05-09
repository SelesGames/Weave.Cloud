using System;

namespace Common.Caching
{
    public class CacheEntry<T>
    {
        public string CacheSourceName { get; set; }
        public T Value { get; set; }
        public DateTime LastAccess { get; set; }
    }
}
