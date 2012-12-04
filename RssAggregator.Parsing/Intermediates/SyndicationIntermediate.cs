using System;
using System.Linq;
using System.ServiceModel.Syndication;

namespace Weave.RssAggregator.Parsing
{
    internal class SyndicationIntermediate : EntryIntermediate
    {
        SyndicationItem syndicationItem;

        public SyndicationIntermediate(SyndicationItem syndicationItem)
        {
            this.syndicationItem = syndicationItem;
        }

        protected override DateTime? GetPublicationDate()
        {
            return syndicationItem.PublishDate.UtcDateTime;
        }

        protected override Entry ParseInternal()
        {
            var link = syndicationItem.Links.FirstOrDefault();
            if (link == null)
                return null;

            string content = null;
            if (syndicationItem.Content is TextSyndicationContent)
                content = ((TextSyndicationContent)syndicationItem.Content).Text;

            var e = new Entry
            {
                Title = syndicationItem.Title.Text,
                Link = link.Uri.AbsoluteUri,
                Description = content,
                PublishDateTime = ParsePubDate(),
                ImageUrl = null,
            };

            e.ExtractYoutubeVideoAndPodcastUrlsFromDescription();

            return e;
        }

        string ParsePubDate()
        {
            if (syndicationItem == null)
                return null;

            return syndicationItem.PublishDate.UtcDateTime.ToString("M/dd/yyyy h:mm:ss tt +00:00");
        }
    }

}
