using System;

namespace Weave.RssAggregator.HighFrequency
{
    public static class HighFrequencyFeedCacheService
    {
        public static HighFrequencyFeedRssCache CurrentCache { get; private set; }

        public static void CreateCache(string feedLibraryUrl, int highFrequencyRefreshSplit, TimeSpan highFrequencyRefreshPeriod)
        {
            if (CurrentCache != null)
                CurrentCache.Dispose();

            CurrentCache = new HighFrequencyFeedRssCache(feedLibraryUrl, highFrequencyRefreshSplit, highFrequencyRefreshPeriod);
        }
    }
}
