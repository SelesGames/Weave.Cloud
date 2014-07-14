using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;

namespace Weave.User.Service.Cache.Mutable
{
    public class NewsItemCache
    {
        //readonly string CACHE_NAME = "user";
        //DataCache cache;
        UserInfoBlobWriteQueue writeQueue;
        UserInfoBlobClient userInfoBlobClient;
        //AzureLocalCache cache;

        public IEnumerable<NewsItem> Get(IEnumerable<Guid> newsItemIds)
        {

        }
    }
}
