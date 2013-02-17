using SelesGames.Common.Hashing;
using System;
using System.Linq;
using System.ServiceModel.Syndication;

namespace Weave.RssAggregator.Parsing
{
    internal class SyndicationIntermediate : IEntryIntermediate
    {
        SyndicationItem syndicationItem;

        public DateTime PublicationDate { get; set; }
        public string PublicationDateString { get; set; }

        public SyndicationIntermediate(SyndicationItem syndicationItem)
        {
            if (syndicationItem == null)
                throw new ArgumentNullException("syndicationItem in SyndicationIntermediate constructor");

            this.syndicationItem = syndicationItem;
            PublicationDate = syndicationItem.PublishDate.UtcDateTime;
            PublicationDateString = syndicationItem.PublishDate.UtcDateTime.ToString("M/dd/yyyy h:mm:ss tt +00:00");
        }

        public Entry CreateEntry()
        {
            var link = syndicationItem.Links.FirstOrDefault();
            if (link == null)
                throw new Exception("invalid link in SyndicationIntermediate.CreateEntry()");

            string content = null;
            if (syndicationItem.Content is TextSyndicationContent)
                content = ((TextSyndicationContent)syndicationItem.Content).Text;

            var e = new Entry
            {
                Title = syndicationItem.Title.Text,
                Link = link.Uri.AbsoluteUri,
                Description = content,
                PublishDateTimeString = PublicationDateString,
                PublishDateTime = PublicationDate,
                ImageUrl = null,
            };

            e.ExtractYoutubeVideoAndPodcastUrlsFromDescription();

            return e;
        }
    }
}
