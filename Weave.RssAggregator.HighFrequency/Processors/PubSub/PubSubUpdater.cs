using StackExchange.Redis;
using System.IO;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;

namespace Weave.RssAggregator.HighFrequency
{
    public class PubSubUpdater : ISequentialAsyncProcessor<FeedUpdate>
    {
        const string CHANNEL = "feedUpdate";

        ConnectionMultiplexer cm;

        public PubSubUpdater(ConnectionMultiplexer cm)
        {
            this.cm = cm;
        }

        public bool IsHandledFully { get; private set; }

        public async Task ProcessAsync(FeedUpdate update)
        {
            byte[] bytes;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(update.Feed.Id.ToByteArray());
                bw.Write(update.RefreshTime.ToBinary());
                bw.Write(update.Feed.Uri);

                bytes = ms.ToArray();
            }

            var sub = cm.GetSubscriber();
            var received = await sub.PublishAsync(CHANNEL, bytes);

            DebugEx.WriteLine("** REDIS PUBSUB ** processed: {0}, {1} clients received", update.Feed.Uri, received);
        }
    }
}