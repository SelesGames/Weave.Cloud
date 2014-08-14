using System;

namespace Weave.Parsing
{
    public class Entry
    {
        public Guid Id { get; set; }
        public Guid FeedId { get; set; }
        public DateTime UtcPublishDateTime { get; set; }

        public string Title { get; set; }
        public string OriginalPublishDateTimeString { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public string YoutubeId { get; set; }
        public string VideoUri { get; set; }
        public string PodcastUri { get; set; }
        public string ZuneAppId { get; set; }
        public string OriginalRssXml { get; set; }
        public Images ImageUrls { get; private set; }

        // display in Universal Sortable format
        // more info http://msdn.microsoft.com/en-us/library/az4se3k1.aspx#UniversalSortable
        public string UtcPublishDateTimeString
        {
            get { return UtcPublishDateTime.ToString("u"); }
        }

        public Entry()
        {
            ImageUrls = new Images();
        }

        public override string ToString()
        {
            return Title;
        }
    }
}