using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.Parsing;

namespace Weave.Updater.BusinessObjects
{
    /// <summary>
    /// A class used for updating a feed.  The functions the class provides are:
    /// 
    /// - To maintain ETag and LastModified data for efficient refreshes of the underlying RSS feed
    /// - To maintain a historical list of the last N news items, sorted by publication date
    /// - To allow for efficient addition of new news articles without duplicates
    /// </summary>
    public class Feed
    {
        public static TimeSpan MinimumElapsedRefreshTime { get; set; }
        static Feed()
        {
            // prevent the feed from being refreshed more frequently than every X minutes
            MinimumElapsedRefreshTime = TimeSpan.FromMinutes(3);
        }

        const int NUMBER_OF_NEWSITEMS_TO_HOLD = 200;

        // Read-only properties
        public string Uri { get; set; }
        public News News { get; private set; }
        public string TeaserImageUrl { get; set; }

        // record-keeping for feed updates
        public DateTime LastRefreshedOn { get; set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }

        public TimeSpan RefreshTimeout { get; set; }




        #region Constructor

        public Feed(string feedUri)
        {
            if (string.IsNullOrWhiteSpace(feedUri)) throw new ArgumentException("feedUri in HighFrequencyFeed ctor");

            Uri = feedUri;
            RefreshTimeout = TimeSpan.FromMinutes(1);
            News = new News();
        }

        #endregion




        public async Task<FeedUpdate> Refresh()
        {
            try
            {
                var now = DateTime.UtcNow;

                if (now - LastRefreshedOn < MinimumElapsedRefreshTime)
                    return null;

                var requester = CreateInnerFeed();
                var result = await requester.Update();

                if (result == Parsing.Feed.RequestStatus.OK)
                {
                    LastRefreshedOn = now;
                    Etag = requester.Etag;
                    LastModified = requester.LastModified;
                    MostRecentNewsItemPubDate = requester.MostRecentNewsItemPubDate;

                    if (requester.News == null || !requester.News.Any())
                    {
                        DebugEx.WriteLine("{0}: Requester had no news, or was not able to parse any news", Uri);
                        return new FeedUpdate { RefreshTime = now, Entries = new List<ExpandedEntry>(), Feed = this };
                    }

                    var resultNews = requester.News.Select(Map).ToList();
                    var addedNews = new List<ExpandedEntry>();

                    foreach (var o in requester.News)
                    {
                        var record = AsRecord(o);
                        if (News.Add(record))
                        {
                            var expandedEntry = Map(o);
                            expandedEntry.Description = null;
                            expandedEntry.OriginalDownloadDateTime = now;
                            addedNews.Add(expandedEntry);
                        }
                    }

                    News.TrimTo(NUMBER_OF_NEWSITEMS_TO_HOLD);

                    if (addedNews.Any())
                    {
                        TeaserImageUrl = addedNews.GetBestImageUrl() ?? TeaserImageUrl;
                    }

                    DebugEx.WriteLine("REFRESHED {0}", Uri);

                    return new FeedUpdate
                    {
                        Feed = this,
                        RefreshTime = now,
                        Entries = addedNews,
                    };
                }
                else if (result == Parsing.Feed.RequestStatus.Unmodified)
                {
                    LastRefreshedOn = now;
                    DebugEx.WriteLine("UNMODIFIED {0}", Uri);
                }
            }
            catch (TaskCanceledException ex) { HandleTimeoutException(ex); }
            catch (Exception ex) { HandleGeneralException(ex); }
            return null;
        }




        #region Private helper functions

        Parsing.Feed CreateInnerFeed()
        {
            return new Parsing.Feed
            {
                Uri = Uri,
                MostRecentNewsItemPubDate = MostRecentNewsItemPubDate,
                Etag = Etag,
                LastModified = LastModified,
                UpdateTimeOut = RefreshTimeout,
                IsAggressiveDomainDiscoveryEnabled = false,
            };
        }

        void HandleTimeoutException(TaskCanceledException ex)
        {
            DebugEx.WriteLine("!!!!!! TIMED OUT {0}: {1}", Uri, ex.Message);
        }

        void HandleGeneralException(Exception ex)
        {
            if (ex.InnerException != null)
                DebugEx.WriteLine("!!!!!! FAILED {0}: {1}: {2}", Uri, ex.Message, ex.InnerException.Message);
            else
                DebugEx.WriteLine("!!!!!! FAILED {0}: {1}", Uri, ex.Message);
        }

        #endregion




        #region Map functions

        static NewsItemRecord AsRecord(Entry o)
        {
            return new NewsItemRecord
            {
                Id = o.Id,
                UtcPublishDateTime = o.UtcPublishDateTime,
                Title = o.Title,
                Link = o.Link,
                HasImage = o.ImageUrls.Any(),
            };
        }

        static void Copy(Entry source, Entry destination)
        {
            var x = source;
            var y = destination;

            y.Id = x.Id;
            y.FeedId = x.FeedId;
            y.UtcPublishDateTime = x.UtcPublishDateTime;
            y.Title = x.Title;
            y.OriginalPublishDateTimeString = x.OriginalPublishDateTimeString;
            y.Link = x.Link;
            y.Description = x.Description;
            y.YoutubeId = x.YoutubeId;
            y.VideoUri = x.VideoUri;
            y.PodcastUri = x.PodcastUri;
            y.ZuneAppId = x.ZuneAppId;
            y.OriginalRssXml = x.OriginalRssXml;

            foreach (var imageUrl in x.ImageUrls)
                y.ImageUrls.Add(imageUrl);
        }

        static ExpandedEntry Map(Entry o)
        {
            var result = new ExpandedEntry();
            Copy(o, result);
            return result;
        }

        #endregion




        public override string ToString()
        {
            return Uri;
        }
    }
}