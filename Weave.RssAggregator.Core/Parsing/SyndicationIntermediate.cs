using System;
using System.Linq;
using System.ServiceModel.Syndication;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.Core.Parsing
{
    internal class SyndicationIntermediate : IRssIntermediate
    {
        SyndicationItem syndicationItem;

        public SyndicationIntermediate(SyndicationItem syndicationItem)
        {
            this.syndicationItem = syndicationItem;
        }

        public Tuple<bool, DateTime> GetTimeStamp()
        {
            if (syndicationItem == null)
                return Tuple.Create(false, DateTime.MinValue);

            return Tuple.Create(true, syndicationItem.PublishDate.UtcDateTime);
        }

        public string GetPublicationDate()
        {
            if (syndicationItem == null)
                return null;

            return syndicationItem.PublishDate.UtcDateTime.ToString("M/dd/yyyy h:mm:ss tt +00:00");
        }

        public NewsItem ToNewsItem()
        {
            var link = syndicationItem.Links.FirstOrDefault();
            if (link == null)
                return null;

            string content = null;
            if (syndicationItem.Content is TextSyndicationContent)
                content = ((TextSyndicationContent)syndicationItem.Content).Text;



            var ni = new NewsItem
            {
                Title = syndicationItem.Title.Text,
                Link = link.Uri.AbsoluteUri,
                Description = content,
                PublishDateTime = GetPublicationDate(),
                ImageUrl = null,
            };

            ni.ExtractYoutubeVideoAndPodcastUrlsFromDescription();

            return ni;
        }
    }

}
