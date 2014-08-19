using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Weave.Parsing;

namespace Weave.Updater.BusinessObjects
{
    public class Feed
    {
        const int NUMBER_OF_NEWSITEMS_TO_HOLD = 200;

        // Read-only properties
        public Guid Id { get; private set; }
        public string Uri { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyList<string> Instructions { get; private set; }
        public FeedState LastFeedState { get; private set; }
        public ExpandedEntries Entries { get; private set; }

        public string TeaserImageUrl { get; set; }

        // record-keeping for feed updates
        public DateTime LastRefreshedOn { get; set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }

        public TimeSpan RefreshTimeout { get; set; }

        public enum FeedState
        {
            Uninitialized,
            Failed,
            OK
        }




        #region Constructor

        public Feed(string name, string feedUri, string originalUri, string instructions)
        {
            if (string.IsNullOrWhiteSpace(name))        throw new ArgumentException("name in HighFrequencyFeed ctor");
            if (string.IsNullOrWhiteSpace(feedUri)) throw new ArgumentException("feedUri in HighFrequencyFeed ctor");

            Name = name;
            Uri = feedUri;
            InitializeId(string.IsNullOrWhiteSpace(originalUri) ? feedUri : originalUri);
            LastFeedState = FeedState.Uninitialized;
            RefreshTimeout = TimeSpan.FromMinutes(1);

            Entries = new ExpandedEntries();

            if (!string.IsNullOrWhiteSpace(instructions))
            {
                Instructions = instructions
                    .Split(',')
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o.Trim())
                    .ToList();
            }
        }

        #endregion




        public async Task<FeedUpdate> Refresh()
        {
            try
            {
                var now = DateTime.UtcNow;

                var requester = CreateInnerFeed();
                var result = await requester.Update();

                if (result == Parsing.Feed.RequestStatus.OK)
                {
                    LastRefreshedOn = now;
                    Etag = requester.Etag;
                    LastModified = requester.LastModified;

                    if (requester.News != null && requester.News.Any())
                    {
                        var resultNews = requester.News.Select(Map).ToList();
                        var addedNews = new List<ExpandedEntry>();

                        foreach (var o in resultNews)
                        {
                            // null out the Description field
                            o.Description = null;
                            o.OriginalDownloadDateTime = now;
                            if (Entries.Add(o))
                                addedNews.Add(o);
                        }

                        Entries.TrimTo(NUMBER_OF_NEWSITEMS_TO_HOLD);

                        if (addedNews.Any())
                        {
                            var update = new FeedUpdate
                            {
                                Feed = this,
                                RefreshTime = now,
                                Entries = addedNews,
                            };
                            return update;
                        }
                    }

                    DebugEx.WriteLine("REFRESHED {0}  ({1})", Name, Uri);
                }
                else if (result == Parsing.Feed.RequestStatus.Unmodified)
                {
                    LastRefreshedOn = now;
                    DebugEx.WriteLine("UNMODIFIED {0}  ({1})", Name, Uri);
                }
                LastFeedState = FeedState.OK;
            }
            catch (TaskCanceledException ex) { HandleTimeoutException(ex); }
            catch (HttpRequestException ex) { HandleHttpRequestException(ex); }
            catch (Exception ex) { HandleGeneralException(ex); }
            return null;
        }




        #region Private helper functions

        void InitializeId(string uri)
        {
            Id = CryptoHelper.ComputeHashUsedByMobilizer(uri);
        }

        Parsing.Feed CreateInnerFeed()
        {
            return new Parsing.Feed
            {
                FeedId = Id,
                FeedUri = Uri,
                MostRecentNewsItemPubDate = MostRecentNewsItemPubDate,
                Etag = Etag,
                LastModified = LastModified,
                UpdateTimeOut = RefreshTimeout,
                IsAggressiveDomainDiscoveryEnabled = false,
            };
        }

        void HandleTimeoutException(TaskCanceledException ex)
        {
            DebugEx.WriteLine("!!!!!! TIMED OUT {0}  ({1}): {2}", Name, Uri, ex.Message);
            LastFeedState = FeedState.Failed;
        }

        void HandleHttpRequestException(HttpRequestException ex)
        {
            if (ex.InnerException != null)
                DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}: {3}", Name, Uri, ex.Message, ex.InnerException.Message);
            else
                DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}", Name, Uri, ex.Message);

            LastFeedState = FeedState.Failed;
        }

        void HandleGeneralException(Exception ex)
        {
            DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}", Name, Uri, ex.Message);
            LastFeedState = FeedState.Failed;
        }

        #endregion




        #region Map functions

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




        void UpdateTeaserImage()
        {
            var image = Entries.SelectMany(o => o.Images).GetBest();
            if (image != null)
                TeaserImageUrl = image.Url;
            //TeaserImageUrl = News
            //    .Where(o => o.HasImage)
            //    .Select(o => o.GetBestImageUrl())
            //    .FirstOrDefault();
        }

        public override string ToString()
        {
            return Name + ": " + Uri;
        }
    }
}
