using System;
using System.Web;
using System.Xml.Linq;

namespace Weave.RssAggregator.Parsing
{
    internal class Rss20Intermediate : EntryIntermediate
    {
        static readonly XNamespace dc = "http://purl.org/dc/elements/1.1/";
        XElement xml;

        public Rss20Intermediate(XElement xmlNode)
        {
            this.xml = xmlNode;
        }

        protected override DateTime? GetPublicationDate()
        {
            var val = ParsePubDateFromXmlNode();
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
                var ni = new Entry();

                var title = xml.Element("title");
                var link = xml.Element("link");

                if (title == null || link == null)
                    return null;

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

                ni.PublishDateTime = ParsePubDateFromXmlNode();

                return ni;
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
    }
}
