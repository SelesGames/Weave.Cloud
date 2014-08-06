using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.Parsing
{
    public class Entry
    {
        List<string> imageUrls = new List<string>();

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

        public IEnumerable<string> ImageUrls { get { return imageUrls; } }


        // display in Universal Sortable format
        // more info http://msdn.microsoft.com/en-us/library/az4se3k1.aspx#UniversalSortable
        public string UtcPublishDateTimeString
        {
            get { return UtcPublishDateTime.ToString("u"); }
        }

        public override string ToString()
        {
            return Title;
        }

        public void AddImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;
            if (!imageUrl.IsImageUrl()) return;

            if (imageUrl.StartsWith("http://share.feedsportal.com/share/", StringComparison.OrdinalIgnoreCase))
                return;

            if (imageUrl.StartsWith("http://res3.feedsportal.com/social/", StringComparison.OrdinalIgnoreCase))
                return;

            imageUrls.Add(imageUrl);
        }

        public string GetImageUrl()
        {
            if (EnumerableEx.IsNullOrEmpty(imageUrls))
                return null;

            return imageUrls.First();
        }
    }
}
