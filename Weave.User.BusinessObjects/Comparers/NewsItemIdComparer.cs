using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.User.BusinessObjects.Comparers
{
    internal class NewsItemIdComparer : IEqualityComparer<NewsItem>
    {
        public bool Equals(NewsItem x, NewsItem y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(NewsItem obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
