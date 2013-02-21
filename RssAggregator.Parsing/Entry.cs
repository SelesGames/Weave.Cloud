using System;

namespace Weave.RssAggregator.Client
{
    public class Entry
    {
        public Guid Id { get; set; }
        public Guid FeedId { get; set; }
        public DateTime PublishDateTime { get; set; }

        public string Title { get; set; }
        public string PublishDateTimeString { get; set; }
        public string Link { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string YoutubeId { get; set; }
        public string VideoUri { get; set; }
        public string PodcastUri { get; set; }
        public string ZuneAppId { get; set; }
        public string OriginalRssXml { get; set; }

        public byte[] NewsItemBlob { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
