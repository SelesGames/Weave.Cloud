using ProtoBuf;
using SelesGames.Common;
using System.IO;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.HighFrequency
{
    public class EntryToBinaryUpdater : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        static EntryToBinaryUpdater()
        {
            Serializer.PrepareSerializer<NewsItem>();
        }

        public EntryToBinaryUpdater()
        {
            IsHandledFully = false;
        }

        public bool IsHandledFully { get; private set; }

        public Task ProcessAsync(HighFrequencyFeedUpdateDto update)
        {
            if (update.Entries == null)
                return Task.FromResult<object>(null);

            foreach (var entry in update.Entries)
            {
                var newsItem = entry.Convert(EntryWithPostProcessInfoToNewsItemConverter.Instance);

                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, newsItem);
                    ms.Position = 0;
                    var byteArray = ms.ToArray();
                    entry.NewsItemBlob = byteArray;
                }
            }

            DebugEx.WriteLine("EntryToBinaryUpdater processed: {0}", update.FeedUri);

            return Task.FromResult<object>(null);
        }
    }
}
