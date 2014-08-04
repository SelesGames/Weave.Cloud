using System.IO;
using System.Threading.Tasks;
using Weave.User.Service.Redis.PubSub;

namespace Weave.RssAggregator.HighFrequency
{
    public class PubSubUpdater : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        const string CHANNEL = "feedUpdate";

        PubSubHelper ps;

        public PubSubUpdater(PubSubHelper ps)
        {
            this.ps = ps;
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

            await ps.Publish(CHANNEL, bytes);

            DebugEx.WriteLine("** REDIS PUBSUB ** processed: {0}", update.FeedUri);
        }
    }
}