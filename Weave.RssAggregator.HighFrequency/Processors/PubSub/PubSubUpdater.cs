using StackExchange.Redis;
using System.Threading.Tasks;
using Weave.Updater.PubSub;

namespace Weave.RssAggregator.HighFrequency
{
    public class PubSubUpdater : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        ConnectionMultiplexer cm;

        public PubSubUpdater(ConnectionMultiplexer cm)
        {
            this.cm = cm;
        }

        public async Task ProcessAsync(HighFrequencyFeedUpdate update)
        {
            var bridge = new FeedUpdateEventBridge(cm);
            var received = await bridge.Publish(update.InnerUpdate);

            DebugEx.WriteLine("** REDIS PUBSUB ** processed: {0}, {1} clients received", update.Feed.Uri, received);
        }
    }
}