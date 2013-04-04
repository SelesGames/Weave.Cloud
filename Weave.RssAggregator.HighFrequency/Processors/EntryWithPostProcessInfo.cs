using System;

namespace Weave.RssAggregator.HighFrequency
{
    public class EntryWithPostProcessInfo
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

        public byte[] NewsItemBlob { get; set; }

        public bool ShouldIncludeImage { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public string OriginalImageUrl { get; set; }
        public string BaseResizedImageUrl { get; set; }
        public string SupportedFormats { get; set; }
        public string PreferredImageUrl { get; set; }


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

        //static Core.DTOs.Outgoing.Aspect ConvertToAspect(string aspectString)
        //{
        //    if ("portrait".Equals(aspectString, StringComparison.OrdinalIgnoreCase))
        //        return Core.DTOs.Outgoing.Aspect.Portrait;

        //    if ("landscape".Equals(aspectString, StringComparison.OrdinalIgnoreCase))
        //        return Core.DTOs.Outgoing.Aspect.Landscape;

        //    if ("square".Equals(aspectString, StringComparison.OrdinalIgnoreCase))
        //        return Core.DTOs.Outgoing.Aspect.Square;

        //    // by default, return landscape aspect
        //    return Core.DTOs.Outgoing.Aspect.Landscape;
        //}
    }
}
