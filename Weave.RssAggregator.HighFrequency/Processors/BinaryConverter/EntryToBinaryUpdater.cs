using ProtoBuf;
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

        static NewsItem Map(EntryWithPostProcessInfo e)
        {
            return new Weave.RssAggregator.Core.DTOs.Outgoing.NewsItem
            {
                Id = e.Id,
                Title = e.Title,
                Link = e.Link,
                ImageUrl = !e.Image.ShouldIncludeImage ? null : e.Image.PreferredUrl,
                PublishDateTime = e.UtcPublishDateTimeString,
                Description = null,
                VideoUri = e.VideoUri,
                YoutubeId = e.YoutubeId,
                PodcastUri = e.PodcastUri,
                ZuneAppId = e.ZuneAppId,
                FeedId = e.FeedId,
                Image = !e.Image.ShouldIncludeImage ? null : Map(e.Image),
            };
        }

        static Image Map(EntryImage o)
        {
            return new Image
            {
                Width = o.Width,
                Height = o.Height,
                BaseImageUrl = o.BaseResizedUrl,
                OriginalUrl = o.OriginalUrl,
                SupportedFormats = o.SupportedFormats,
            };
        }

        #endregion
    }
}
