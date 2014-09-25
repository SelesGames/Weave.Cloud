using System.Collections.Generic;

namespace Weave.FeedUpdater.BusinessObjects.Cache
{
    public class CacheSaveResult
    {
        public IEnumerable<bool> RedisSaves { get; set; }
        public IEnumerable<bool> BlobSaves { get; set; }
        public dynamic Meta { get; set; }
    }
}