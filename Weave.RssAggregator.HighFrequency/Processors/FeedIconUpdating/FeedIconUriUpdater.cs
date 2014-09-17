using RssAggregator.IconCaching;
using System.Threading.Tasks;

namespace Weave.FeedUpdater.HighFrequency
{
    public class FeedIconUriUpdater : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        static StandardIconCache iconCache = new StandardIconCache();

        public async Task ProcessAsync(HighFrequencyFeedUpdate update)
        {
            var feed = update.InnerFeed;

            var iconUri = await iconCache.Get(feed.Uri);
            if (!string.IsNullOrWhiteSpace(iconUri))
                feed.IconUri = iconUri;
        }
    }
}