using System;
using System.Linq;
using System.Threading.Tasks;

namespace Weave.RssAggregator.LowFrequency
{
    public class HFeedDbMediator
    {
        CachedFeed feed;

        public DateTime LastRefresh { get; private set; }
        public Exception CurrentLoadLatestException { get; private set; }

        public HFeedDbMediator(CachedFeed feed)
        {
            if (feed == null) throw new ArgumentNullException("feed in HFeedDbMediator ctor");

            this.feed = feed;

            LastRefresh = DateTime.MinValue;
        }

        public async void ProcessFeedUpdateNotice(FeedUpdateNotice notice)
        {
            try
            {
                if (notice == null)
                    return;

                if (notice.FeedUri.Equals(feed.Uri, StringComparison.OrdinalIgnoreCase))
                {
                    if (notice.RefreshTime > LastRefresh)
                    {
                        await LoadLatestNews();
                    }
                }
            }
#if DEBUG
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
            }
#else
            catch { }
#endif
        }
    }
}
