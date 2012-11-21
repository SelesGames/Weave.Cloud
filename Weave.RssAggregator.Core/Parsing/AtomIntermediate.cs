using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.Core.Parsing
{
    public class AtomIntermediate : IRssIntermediate
    {
        static readonly XNamespace atom = "http://www.w3.org/2005/Atom";

        XElement xmlNode;
        string pubDate;

        public AtomIntermediate(XElement xmlNode)
        {
            this.xmlNode = xmlNode;
        }

        public Tuple<bool, DateTime> GetTimeStamp()
        {
            if (pubDate == null)
                pubDate = xmlNode.Element(atom + "published").ValueOrDefault();
            return TryGetUtcDate(pubDate);
        }

        public string GetPublicationDate()
        {
            if (pubDate == null)
                return xmlNode.Element(atom + "published").ValueOrDefault();
            else
                return pubDate;
        }




        #region static helper classes for parsing the NewsItem from the XML (XElement)

        static NewsItem ToNewsItem(XElement item)
        {
            try
            {
                NewsItem ni = new NewsItem();

                var title = item.Element(atom + "title");
                var link = item.Element(atom + "link");

                if (title == null || link == null)
                    return null;

                ni.Title = HttpUtility.HtmlDecode(title.Value.Trim());
                var href = link.Attributes().Where(o => o.Name == "href").SingleOrDefault();
                ni.Link = href.ValueOrDefault();
                if (string.IsNullOrEmpty(ni.Link))
                    return null;


                XElement descriptionNode;
                var content = item.GetContentNode();
                if (content != null)
                    descriptionNode = content;
                else
                    descriptionNode = item.Element(atom + "content");

                var description = descriptionNode.ValueOrDefault();

                ni.Description = description;



                // attempt to parse the description html for various media links
                ni.ExtractYoutubeVideoAndPodcastUrlsFromDescription();



                // if either image or podcast or video is not set, check for media content
                if (string.IsNullOrEmpty(ni.ImageUrl) || string.IsNullOrEmpty(ni.PodcastUri) || string.IsNullOrEmpty(ni.VideoUri))
                {
                    var mediaContent = item.GetMediaContentUrl();

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
                    var enclosure = item.GetEnclosureUrl();

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



                ni.PublishDateTime = item.Element(atom + "published").ValueOrDefault();

                return ni;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion




        public NewsItem ToNewsItem()
        {
            return ToNewsItem(xmlNode);
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
