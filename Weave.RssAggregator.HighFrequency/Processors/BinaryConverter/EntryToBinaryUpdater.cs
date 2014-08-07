using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Outgoing = Weave.RssAggregator.Core.DTOs.Outgoing;
using System.Linq;

namespace Weave.RssAggregator.HighFrequency
{
    public class EntryToBinaryUpdater : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        static EntryToBinaryUpdater()
        {
            Serializer.PrepareSerializer<Outgoing.NewsItem>();
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
                var newsItem = Map(entry);

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




        #region Map functions

        static Outgoing.NewsItem Map(EntryWithPostProcessInfo e)
        {
            var bestImage = e.Images.GetBest();

            return new Outgoing.NewsItem
            {
                Id = e.Id,
                Title = e.Title,
                Link = e.Link,
                ImageUrl = bestImage == null ? null : bestImage.Url,
                PublishDateTime = e.UtcPublishDateTimeString,
                Description = null,
                VideoUri = e.VideoUri,
                YoutubeId = e.YoutubeId,
                PodcastUri = e.PodcastUri,
                ZuneAppId = e.ZuneAppId,
                FeedId = e.FeedId,
                Image = bestImage == null ? null : Map(bestImage),
            };
        }

        static Outgoing.Image Map(Image o)
        {
            return new Outgoing.Image
            {
                Width = o.Width,
                Height = o.Height,
                OriginalUrl = o.Url,
                BaseImageUrl = null,
                SupportedFormats = null,
            };
        }

        #endregion
    }
}
