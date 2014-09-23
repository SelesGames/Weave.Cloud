using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.FeedUpdater.BusinessObjects.Cache
{
    public class CacheSaveResult
    {
        public IEnumerable<bool> RedisSaves { get; set; }
        public IEnumerable<bool> BlobSaves { get; set; }
    }
}
