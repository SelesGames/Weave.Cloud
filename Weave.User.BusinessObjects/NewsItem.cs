using Common.TimeFormatting;
using System;

namespace Weave.User.BusinessObjects
{
    public class NewsItem
    {
        string utcPublishDateTimeString;

        public bool FailedToParseUtcPublishDateTime { get; private set; }

        public Guid Id { get; set; }
        public Feed Feed { get; set; }
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

        public string UtcPublishDateTimeString
        {
            get { return utcPublishDateTimeString; }
            set
            {
                utcPublishDateTimeString = value;
                UpdateDateTime();
            }
        }

        public string GetBestImageUrl()
        {
            if (Image != null)
            {
                return Image.CreateImageUrl();
            }
            return ImageUrl;
        }

        public bool IsNew()
        {
            //return !HasBeenViewed && OriginalDownloadDateTime > Feed.PreviousEntrance;
            return OriginalDownloadDateTime > Feed.PreviousEntrance;
        }




        #region Derived Properties (readonly)

        public DateTime UtcPublishDateTime { get; private set; }

        public bool HasImage
        {
            get
            {
                return !string.IsNullOrEmpty(ImageUrl) &&
                    Uri.IsWellFormedUriString(ImageUrl, UriKind.Absolute);
            }
        }

        public double SortRating
        {
            get { return CalculateSortRating(UtcPublishDateTime); }
        }

        #endregion




        #region Helper methods

        void UpdateDateTime()
        {
            var attempt = utcPublishDateTimeString.TryGetUtcDate();
            if (attempt.Item1)
                UtcPublishDateTime = attempt.Item2;
            else
                FailedToParseUtcPublishDateTime = true;
        }

        static double CalculateSortRating(DateTime dateTime)
        {
            double elapsedHours = (DateTime.UtcNow - dateTime).TotalHours;
            if (elapsedHours <= 0)
                elapsedHours = 0.0001;
            double value = 1d / elapsedHours;
            return value;
        }

        #endregion
    }
}
