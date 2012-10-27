using System;

namespace Weave.Mobilizer.Core.Cache
{
    public class TimeStampedCacheEntry<T>
    {
        public T Value { get; set; }
        public DateTime LastAccess { get; set; }
    }
}
