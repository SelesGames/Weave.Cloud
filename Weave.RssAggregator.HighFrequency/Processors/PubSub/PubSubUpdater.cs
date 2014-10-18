using StackExchange.Redis;
using System.Threading.Tasks;
using Weave.FeedUpdater.Messaging;
using Weave.Services.Redis.Ambient;

namespace Weave.FeedUpdater.HighFrequency
{
    public class PubSubUpdater : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        static readonly FeedUpdateMessageQueue QUEUE = new FeedUpdateMessageQueue();

        public async Task ProcessAsync(HighFrequencyFeedUpdate update)
        {
            await QUEUE.Push(update.InnerUpdate);

            DebugEx.WriteLine("** REDIS PUBSUB ** processed: {0}", update.Feed.Uri);
        }
    }
}