using Common.TimeFormatting;
using System;

namespace Weave.UserFeedAggregator.BusinessObjects
{
    public class NewsItem
    {
        string utcPublishDateTimeString;

        public bool FailedToParseUtcPublishDateTime { get; private set; }

        public Guid Id { get; set; }
        public Guid FeedId { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string ImageUrl { get; set; }
        public string YoutubeId { get; set; }
        public string VideoUri { get; set; }
        public string PodcastUri { get; set; }
        public string ZuneAppId { get; set; }
        public bool IsFavorite { get; set; }
        public bool HasBeenViewed { get; set; }
        public DateTime OriginalDownloadDateTime { get; set; }
        public Image Image { get; set; }

        public DateTime UtcPublishDateTime { get; private set; }

        public string UtcPublishDateTimeString
        {
            get { return utcPublishDateTimeString; }
            set
            {
                utcPublishDateTimeString = value;
                UpdateDateTime();
            }
        }

        void UpdateDateTime()
        {
            var attempt = utcPublishDateTimeString.TryGetUtcDate();
            if (attempt.Item1)
                UtcPublishDateTime = attempt.Item2;
            else
                FailedToParseUtcPublishDateTime = true;
        }
    }
}
