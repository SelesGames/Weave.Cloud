using Common.TimeFormatting;
using System;
using System.Web;
using System.Xml.Linq;

namespace Weave.Parsing.Intermediates
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
            var e = new Entry();

            var title = xml.Element("title");
            var link = xml.Element("link");

            if (title == null || link == null)
                throw new Exception("title or link missing - Rss20Intermediate.CreateEntry()");

            e.Title = HttpUtility.HtmlDecode(title.Value.Trim());
            e.Link = link.Value;


            XElement descriptionNode;
            var content = xml.GetContentNode();
            if (content != null)
                descriptionNode = content;
            else
                descriptionNode = xml.Element("description");

            var description = descriptionNode.ValueOrDefault();

            e.Description = description;


            // attempt to parse the description html for various media links
            e.ExtractImagesAndYoutubeVideoAndPodcastUrlsFromDescription();


            // if either image or podcast or video is not set, check for media content
            var mediaContent = xml.GetMediaContentUrl();
            if (mediaContent.IsWellFormed())
            {
                if (mediaContent.IsImageUrl())
                    e.AddImage(mediaContent);

                else if (string.IsNullOrEmpty(e.PodcastUri) && mediaContent.IsAudioFileUrl())
                    e.PodcastUri = mediaContent;

                else if (string.IsNullOrEmpty(e.VideoUri) && mediaContent.IsVideoFileUrl())
                    e.VideoUri = mediaContent;
            }


            // if either image or podcast or video is not set, check the enclosure element
            var enclosure = xml.GetEnclosureUrl();
            if (enclosure.IsWellFormed())
            {
                if (enclosure.IsImageUrl())
                    e.AddImage(enclosure);

                else if (string.IsNullOrEmpty(e.PodcastUri) && enclosure.IsAudioFileUrl())
                    e.PodcastUri = enclosure;

                else if (string.IsNullOrEmpty(e.VideoUri) && enclosure.IsVideoFileUrl())
                    e.VideoUri = enclosure;
            }

            e.AddImage(description.ParseImageUrlFromHtml());

            e.OriginalPublishDateTimeString = PublicationDateString;
            e.UtcPublishDateTime = PublicationDate;
            e.OriginalRssXml = xml.ToString();


            return e;
        }
    }
}
