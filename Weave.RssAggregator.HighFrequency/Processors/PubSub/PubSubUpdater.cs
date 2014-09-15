using StackExchange.Redis;
using System.Threading.Tasks;
using Weave.Services.Redis.Ambient;
using Weave.Updater.PubSub;

namespace Weave.RssAggregator.HighFrequency
{
    public class PubSubUpdater : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        readonly ConnectionMultiplexer cm;

        public PubSubUpdater()
        {
            this.cm = Settings.PubsubConnection;
        }

        public async Task ProcessAsync(HighFrequencyFeedUpdate update)
        {
            var publisher = new FeedUpdatePublisher(cm);
            var received = await publisher.Publish(update.InnerUpdate);

            DebugEx.WriteLine("** REDIS PUBSUB ** processed: {0}, {1} clients received", update.Feed.Uri, received);
        }
    }
}