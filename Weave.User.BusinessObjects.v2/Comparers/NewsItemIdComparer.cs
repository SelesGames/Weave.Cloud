using System.Collections.Generic;

namespace Weave.User.BusinessObjects.v2.Comparers
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
