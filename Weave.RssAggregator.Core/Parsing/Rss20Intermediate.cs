using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.Core.Parsing
{
    public class Rss20Intermediate : IRssIntermediate
    {
        static readonly XNamespace dc = "http://purl.org/dc/elements/1.1/";

        XElement xmlNode;
        string pubDate;

        public Rss20Intermediate(XElement xmlNode)
        {
            this.xmlNode = xmlNode;
        }

        public Tuple<bool, DateTime> GetTimeStamp()
        {
            if (pubDate == null)
                ParsePubDateFromXmlNode();

            return TryGetUtcDate(pubDate);
        }

        public string GetPublicationDate()
        {
            if (pubDate == null)
                ParsePubDateFromXmlNode();
                
            return pubDate;
        }

        void ParsePubDateFromXmlNode()
        {
            var pubDate = xmlNode.Element("pubDate");
            if (pubDate != null)
            {
                this.pubDate = pubDate.Value;
                return;
            }

            pubDate = xmlNode.Element(dc + "date");
            if (pubDate != null)
            {
                this.pubDate = pubDate.Value;
                return;
            }

            this.pubDate = "";
        }

        public NewsItem ToNewsItem()
        {
            try
            {
                NewsItem ni = new NewsItem();

                var title = xmlNode.Element("title");
                var link = xmlNode.Element("link");

                if (title == null || link == null)
                    return null;

                ni.Title = HttpUtility.HtmlDecode(title.Value.Trim());
                ni.Link = link.Value;


                XElement descriptionNode;
                var content = xmlNode.GetContentNode();
                if (content != null)
                    descriptionNode = content;
                else
                    descriptionNode = xmlNode.Element("description");

                var description = descriptionNode.ValueOrDefault();

                ni.Description = description;


                // attempt to parse the description html for various media links
                ni.ExtractYoutubeVideoAndPodcastUrlsFromDescription();


                // if either image or podcast or video is not set, check for media content
                if (string.IsNullOrEmpty(ni.ImageUrl) || string.IsNullOrEmpty(ni.PodcastUri) || string.IsNullOrEmpty(ni.VideoUri))
                {
                    var mediaContent = xmlNode.GetMediaContentUrl();

                    if (mediaContent.IsWellFormed())
                    {
                        if (string.IsNullOrEmpty(ni.ImageUrl) && mediaContent.IsImageUrl())
                            ni.ImageUrl = mediaContent;

                        else if (string.IsNullOrEmpty(ni.PodcastUri) && mediaContent.IsAudioFileUrl())
                            ni.PodcastUri = mediaContent;

                        else if (string.IsNullOrEmpty(ni.VideoUri) && mediaContent.IsVideoFileUrl())
                            ni.VideoUri = mediaContent;
                    }
                }


                // if either image or podcast or video is not set, check the enclosure element
                if (string.IsNullOrEmpty(ni.ImageUrl) || string.IsNullOrEmpty(ni.PodcastUri) || string.IsNullOrEmpty(ni.VideoUri))
                {
                    var enclosure = xmlNode.GetEnclosureUrl();

                    if (enclosure.IsWellFormed())
                    {
                        if (string.IsNullOrEmpty(ni.ImageUrl) && enclosure.IsImageUrl())
                            ni.ImageUrl = enclosure;

                        else if (string.IsNullOrEmpty(ni.PodcastUri) && enclosure.IsAudioFileUrl())
                            ni.PodcastUri = enclosure;

                        else if (string.IsNullOrEmpty(ni.VideoUri) && enclosure.IsVideoFileUrl())
                            ni.VideoUri = enclosure;
                    }
                }

                if (string.IsNullOrEmpty(ni.ImageUrl))
                    ni.ImageUrl = description.ParseImageUrlFromHtml();

                ni.PublishDateTime = GetPublicationDate();

                return ni;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Tuple<bool, DateTime> TryGetUtcDate(string dateTimeString)
        {
            try
            {
                if (string.IsNullOrEmpty(dateTimeString))
                    return Tuple.Create(false, DateTime.MinValue);

                DateTime dateTime;

                var canRfcParse = SyndicationDateTimeUtility
                    .TryParseRfc822DateTime(dateTimeString, out dateTime);

                if (!canRfcParse)
                {
                    var canNormalParse = DateTime.TryParse(dateTimeString, out dateTime);
                    if (!canNormalParse)
                    {
                        string canParseAny = new[] { "ddd, dd MMM yyyy HH:mm:ss ZK", "yyyy-MM-ddTHH:mm:ssK" }
                            .FirstOrDefault(o => DateTime.TryParseExact(dateTimeString, o, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime));

                        if (canParseAny == null)
                            return Tuple.Create(false, DateTime.MinValue);
                    }
                }

                //if (dateTime.Kind == DateTimeKind.Local)
                //{
                //    var local = TimeZone.CurrentTimeZone;
                //    dateTime = local.ToUniversalTime(dateTime);
                //    //dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
                //}

                //else if (dateTime.Kind == DateTimeKind.Unspecified)
                //    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

                //if (dateTime > DateTime.UtcNow)
                //    dateTime = DateTime.UtcNow;

                return Tuple.Create(true, dateTime);
            }
            catch (Exception)
            {
                return Tuple.Create(false, DateTime.MinValue);
            }
        }
    }
}
