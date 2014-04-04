using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.TimeFormatting;

namespace Weave.User.BusinessObjects.v2
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
            return !HasBeenViewed && OriginalDownloadDateTime > Feed.PreviousEntrance;
        }

        public bool IsCountedAsNew()
        {
            return !HasBeenViewed && OriginalDownloadDateTime > Feed.MostRecentEntrance;
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
                // we have to call ".ToUniversalTime()" because .NET automatically converts the string to be in local time
                UtcPublishDateTime = attempt.Item2.ToUniversalTime();
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




        #region ToString override

        public override string ToString()
        {
            return string.Format("{0}: {1}", Id.ToString("N"), Title);
        }

        #endregion
    }
}
