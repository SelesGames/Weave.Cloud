using System.Threading.Tasks;
using Weave.Updater.PubSub;

namespace Weave.RssAggregator.HighFrequency
{
    public class PubSubUpdater : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        FeedUpdatePublisher publisher;

        public PubSubUpdater(FeedUpdatePublisher publisher)
        {
            this.publisher = publisher;
        }

        public async Task ProcessAsync(HighFrequencyFeedUpdate update)
        {
            var received = await publisher.Publish(update.InnerUpdate);

            DebugEx.WriteLine("** REDIS PUBSUB ** processed: {0}, {1} clients received", update.Feed.Uri, received);
        }
    }
}