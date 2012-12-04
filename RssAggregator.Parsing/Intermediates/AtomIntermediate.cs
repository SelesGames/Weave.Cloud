using System;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Weave.RssAggregator.Parsing
{
    internal class AtomIntermediate : EntryIntermediate
    {
        static readonly XNamespace atom = "http://www.w3.org/2005/Atom";
        XElement xml;

        public AtomIntermediate(XElement xml)
        {
            this.xml = xml;
        }

        protected override DateTime? GetPublicationDate()
        {
            var val = xml.Element(atom + "published").ValueOrDefault();
            if (val != null)
            {
                var pubDate = val.TryGetUtcDate();
                if (pubDate.Item1)
                    return pubDate.Item2;
            }
            return null;
        }

        protected override Entry ParseInternal()
        {
            Entry e = new Entry();

            var title = xml.Element(atom + "title");
            var link = xml.Element(atom + "link");

            if (title == null || link == null)
                return null;

            e.Title = HttpUtility.HtmlDecode(title.Value.Trim());
            var href = link.Attributes().Where(o => o.Name == "href").SingleOrDefault();
            e.Link = href.ValueOrDefault();
            if (string.IsNullOrEmpty(e.Link))
                return null;


            XElement descriptionNode;
            var content = xml.GetContentNode();
            if (content != null)
                descriptionNode = content;
            else
                descriptionNode = xml.Element(atom + "content");

            var description = descriptionNode.ValueOrDefault();

            e.Description = description;


            // attempt to parse the description html for various media links
            e.ExtractYoutubeVideoAndPodcastUrlsFromDescription();


            // if either image or podcast or video is not set, check for media content
            if (string.IsNullOrEmpty(e.ImageUrl) || string.IsNullOrEmpty(e.PodcastUri) || string.IsNullOrEmpty(e.VideoUri))
            {
                var mediaContent = xml.GetMediaContentUrl();

                if (mediaContent.IsWellFormed())
                {
                    if (string.IsNullOrEmpty(e.ImageUrl) && mediaContent.IsImageUrl())
                        e.ImageUrl = mediaContent;

                    else if (string.IsNullOrEmpty(e.PodcastUri) && mediaContent.IsAudioFileUrl())
                        e.PodcastUri = mediaContent;

                    else if (string.IsNullOrEmpty(e.VideoUri) && mediaContent.IsVideoFileUrl())
                        e.VideoUri = mediaContent;
                }
            }


            // if either image or podcast or video is not set, check the enclosure element
            if (string.IsNullOrEmpty(e.ImageUrl) || string.IsNullOrEmpty(e.PodcastUri) || string.IsNullOrEmpty(e.VideoUri))
            {
                var enclosure = xml.GetEnclosureUrl();

                if (enclosure.IsWellFormed())
                {
                    if (string.IsNullOrEmpty(e.ImageUrl) && enclosure.IsImageUrl())
                        e.ImageUrl = enclosure;

                    else if (string.IsNullOrEmpty(e.PodcastUri) && enclosure.IsAudioFileUrl())
                        e.PodcastUri = enclosure;

                    else if (string.IsNullOrEmpty(e.VideoUri) && enclosure.IsVideoFileUrl())
                        e.VideoUri = enclosure;
                }
            }


            if (string.IsNullOrEmpty(e.ImageUrl))
                e.ImageUrl = description.ParseImageUrlFromHtml();


            e.PublishDateTime = xml.Element(atom + "published").ValueOrDefault();

            return e;
        }
    }
}
