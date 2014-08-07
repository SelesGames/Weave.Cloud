using Common.TimeFormatting;
using System;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Weave.Parsing.Intermediates
{
    internal class AtomIntermediate : IEntryIntermediate
    {
        static readonly XNamespace atom = "http://www.w3.org/2005/Atom";
        XElement xml;

        public DateTime PublicationDate { get; set; }
        public string PublicationDateString { get; set; }

        public AtomIntermediate(XElement xml)
        {
            this.xml = xml;
            InitializePublicationDate();
        }




        #region helper functions for setting the PublicationDate field

        void InitializePublicationDate()
        {
            try
            {
                PublicationDateString = xml.Element(atom + "published").Value;
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

        void InitializePubDateToCurrentTime()
        {
            PublicationDate = DateTime.UtcNow;
            PublicationDateString = PublicationDate.ToString();
        }

        #endregion




        public Entry CreateEntry()
        {
            Entry e = new Entry();

            var title = xml.Element(atom + "title");
            var link = xml.Element(atom + "link");

            if (title == null || link == null)
                throw new Exception("title or link missing - AtomIntermediate.CreateEntry()");

            e.Title = HttpUtility.HtmlDecode(title.Value.Trim());
            var href = link.Attributes().Where(o => o.Name == "href").SingleOrDefault();
            e.Link = href.ValueOrDefault();
            if (string.IsNullOrEmpty(e.Link))
                throw new Exception("link missing - AtomIntermediate.CreateEntry()");


            XElement descriptionNode;
            var content = xml.GetContentNode();
            if (content != null)
                descriptionNode = content;
            else
                descriptionNode = xml.Element(atom + "content");

            var description = descriptionNode.ValueOrDefault();

            e.Description = description;


            // attempt to parse the description html for various media links
            e.ExtractImagesAndYoutubeVideoAndPodcastUrlsFromDescription();


            // if either image or podcast or video is not set, check for media content
            var mediaContent = xml.GetMediaContentUrl();
            if (mediaContent.IsWellFormed())
            {
                if (mediaContent.IsImageUrl())
                    e.ImageUrls.Add(mediaContent);

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
                    e.ImageUrls.Add(enclosure);

                else if (string.IsNullOrEmpty(e.PodcastUri) && enclosure.IsAudioFileUrl())
                    e.PodcastUri = enclosure;

                else if (string.IsNullOrEmpty(e.VideoUri) && enclosure.IsVideoFileUrl())
                    e.VideoUri = enclosure;
            }

            e.ImageUrls.Add(description.ParseImageUrlFromHtml());

            e.OriginalPublishDateTimeString = PublicationDateString;
            e.UtcPublishDateTime = PublicationDate;
            e.OriginalRssXml = xml.ToString();

            return e;
        }
    }
}
