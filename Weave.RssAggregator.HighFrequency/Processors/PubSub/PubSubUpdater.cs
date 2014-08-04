using StackExchange.Redis;
using System.IO;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class PubSubUpdater : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        const string CHANNEL = "feedUpdate";

        ConnectionMultiplexer cm;

        public PubSubUpdater(ConnectionMultiplexer cm)
        {
            this.cm = cm;
        }

        public bool IsHandledFully { get; private set; }

        public async Task ProcessAsync(HighFrequencyFeedUpdateDto update)
        {
            byte[] bytes;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(update.FeedId.ToByteArray());
                bw.Write(update.RefreshTime.ToBinary());
                bw.Write(update.FeedUri);

                bytes = ms.ToArray();
            }

            var sub = cm.GetSubscriber();
            var received = await sub.PublishAsync(CHANNEL, bytes);

            DebugEx.WriteLine("** REDIS PUBSUB ** processed: {0}, {1} clients received", update.FeedUri, received);
        }
    }
}