using SelesGames.Common.Hashing;
using System;
using System.Web;
using System.Xml.Linq;

namespace Weave.RssAggregator.Parsing
{
    internal class Rss20Intermediate : IEntryIntermediate
    {
        static readonly XNamespace dc = "http://purl.org/dc/elements/1.1/";
        XElement xml;

        public DateTime PublicationDate { get; set; }
        public string PublicationDateString { get; set; }

        public Rss20Intermediate(XElement xmlNode)
        {
            if (xmlNode == null)
                throw new ArgumentNullException("xmlNode in Rss20Intermediate constructor");

            this.xml = xmlNode;
            InitializePublicationDate();
        }




        #region helper functions for setting the PublicationDate field

        void InitializePublicationDate()
        {
            try
            {
                PublicationDateString = ParsePubDateFromXmlNode();
                var tryParse = PublicationDateString.TryGetUtcDate();
                if (tryParse.Item1)
                    PublicationDate = tryParse.Item2;
                else
                    InitializePubDateToCurrentTime();
            }
            catch
            {
                InitializePubDateToCurrentTime();
            }
        }

        string ParsePubDateFromXmlNode()
        {
            var pubDate = xml.Element("pubDate");
            if (pubDate != null)
                return pubDate.Value;

            pubDate = xml.Element(dc + "date");
            if (pubDate != null)
                return pubDate.Value;

            return null;
        }

        void InitializePubDateToCurrentTime()
        {
            PublicationDate = DateTime.UtcNow;
            PublicationDateString = PublicationDate.ToString();
        }

        #endregion




        public Entry CreateEntry()
        {
            var ni = new Entry();

            var title = xml.Element("title");
            var link = xml.Element("link");

            if (title == null || link == null)
                throw new Exception("title or link missing - Rss20Intermediate.CreateEntry()");

            ni.Title = HttpUtility.HtmlDecode(title.Value.Trim());
            ni.Link = link.Value;


            XElement descriptionNode;
            var content = xml.GetContentNode();
            if (content != null)
                descriptionNode = content;
            else
                descriptionNode = xml.Element("description");

            var description = descriptionNode.ValueOrDefault();

            ni.Description = description;


            // attempt to parse the description html for various media links
            ni.ExtractYoutubeVideoAndPodcastUrlsFromDescription();


            // if either image or podcast or video is not set, check for media content
            if (string.IsNullOrEmpty(ni.ImageUrl) || string.IsNullOrEmpty(ni.PodcastUri) || string.IsNullOrEmpty(ni.VideoUri))
            {
                var mediaContent = xml.GetMediaContentUrl();

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
                var enclosure = xml.GetEnclosureUrl();

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

            ni.PublishDateTimeString = PublicationDateString;
            ni.PublishDateTime = PublicationDate;
            ni.OriginalRssXml = xml.ToString();
            ni.Id = CryptoHelper.ComputeHashUsedByMobilizer(ni.Link);


            return ni;
        }
    }
}
