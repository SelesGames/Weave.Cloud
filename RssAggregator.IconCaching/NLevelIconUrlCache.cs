using Common.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RssAggregator.IconCaching
{
    public class NLevelIconUrlCache : IBasicCache<string, Task<string>>
    {
        public Task<string> Get(string key)
        {
            throw new NotImplementedException();
        }
    }
}
