using RssAggregator.IconCaching;
using System;
using System.Threading.Tasks;

namespace Weave.FeedUpdater.HighFrequency
{
    public class FeedIconUriUpdater : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        readonly StandardIconCache iconCache;

        public FeedIconUriUpdater()
        {
            this.iconCache = new StandardIconCache();
        }

        public async Task ProcessAsync(HighFrequencyFeedUpdate update)
        {
            try
            {
                var feed = update.InnerFeed;

                var iconUri = await iconCache.Get(feed.Uri);
                if (!string.IsNullOrWhiteSpace(iconUri))
                    feed.IconUri = iconUri;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("\r\n\r\n**** FeedIconUriUpdater ERROR ****");
                DebugEx.WriteLine(ex);
            }
        }
    }
}