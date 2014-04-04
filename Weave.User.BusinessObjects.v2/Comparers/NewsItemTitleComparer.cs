using System;
using System.Collections.Generic;

namespace Weave.User.BusinessObjects.v2.Comparers
{
    internal class NewsItemTitleComparer : IEqualityComparer<NewsItem>
    {
        readonly double timeThresholdInHours;

        public NewsItemTitleComparer(double timeThresholdInHours = 48d)
        {
            this.timeThresholdInHours = timeThresholdInHours;
        }

        public bool Equals(NewsItem x, NewsItem y)
        {
            return x.Title.Equals(y.Title) && 
                RelaxedDateTimeEquality(x.UtcPublishDateTime, y.UtcPublishDateTime);
        }

        public int GetHashCode(NewsItem obj)
        {
            return obj.Title.GetHashCode();
        }

        bool RelaxedDateTimeEquality(DateTime x, DateTime y) // if it is 10 minutes diff or less, treat as equal
        {
            return Math.Abs((x - y).TotalHours) <= timeThresholdInHours;
        }
    }
}
