using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Weave.Parsing;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeed
    {
        const int NUMBER_OF_NEWSITEMS_TO_HOLD = 200;

        Subject<HighFrequencyFeedUpdateDto> feedUpdate = new Subject<HighFrequencyFeedUpdateDto>();
        IReadOnlyList<string> instructions;


        public Guid Id { get; private set; }
        public string Uri { get; private set; }
        public string Name { get; private set; }
        public string TeaserImageUrl { get; set; }

        // record-keeping for feed updates
        public DateTime LastRefreshedOn { get; set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }

        public TimeSpan RefreshTimeout { get; set; }
        public FeedState LastFeedState { get; private set; }
        public HFFNews News { get; private set; }

        public IObservable<HighFrequencyFeedUpdateDto> FeedUpdate { get; private set; }

        public enum FeedState
        {
            Uninitialized,
            Failed,
            OK
        }

        public HighFrequencyFeed(string name, string feedUri, string originalUri, string instructions)
        {
            if (string.IsNullOrWhiteSpace(name))        throw new ArgumentException("name in HighFrequencyFeed ctor");
            if (string.IsNullOrWhiteSpace(feedUri)) throw new ArgumentException("feedUri in HighFrequencyFeed ctor");

            Name = name;
            Uri = feedUri;
            InitializeId(string.IsNullOrWhiteSpace(originalUri) ? feedUri : originalUri);
            LastFeedState = FeedState.Uninitialized;
            FeedUpdate = feedUpdate.AsObservable();
            RefreshTimeout = TimeSpan.FromMinutes(1);

            News = new HFFNews();

            if (!string.IsNullOrWhiteSpace(instructions))
            {
                this.instructions = instructions
                    .Split(',')
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o.Trim())
                    .ToList();
            }
        }

        public async Task Refresh()
        {
            try
            {
                var refreshTime = DateTime.UtcNow;

                var requester = new Feed
                {
                    FeedId = this.Id,
                    FeedUri = this.Uri,
                    Etag = this.Etag,
                    LastModified = this.LastModified,
                    UpdateTimeOut = this.RefreshTimeout, 
                };
                var result = await requester.Update();

                if (result == Feed.RequestStatus.OK)
                {
                    this.Etag = requester.Etag;
                    this.LastModified = requester.LastModified;

                    if (requester.News != null && requester.News.Any())
                    {
                        var resultNews = requester.News.Select(Map).ToList();
                        var addedNews = new List<EntryWithPostProcessInfo>();

                        var now = DateTime.UtcNow;
                        foreach (var o in resultNews)
                        {
                            o.OriginalDownloadDateTime = now;
                            if (News.Add(o))
                                addedNews.Add(o);
                        }

                        News.TrimTo(NUMBER_OF_NEWSITEMS_TO_HOLD);

                        if (addedNews.Any())
                        {
                            var update = new HighFrequencyFeedUpdateDto
                            {
                                Feed = this,
                                FeedId = Id,
                                Name = Name,
                                FeedUri = Uri,
                                RefreshTime = refreshTime,
                                Instructions = instructions,
                                Entries = addedNews,
                            };
                            feedUpdate.OnNext(update);
                        }
                    }

                    DebugEx.WriteLine("REFRESHED {0}  ({1})", Name, Uri);
                }
                else if (result == Feed.RequestStatus.Unmodified)
                {
                    DebugEx.WriteLine("UNMODIFIED {0}  ({1})", Name, Uri);
                }
                this.LastFeedState = FeedState.OK;
            }
            catch (TaskCanceledException ex)
            {
                DebugEx.WriteLine("!!!!!! TIMED OUT {0}  ({1}): {2}", Name, Uri, ex.Message);
                this.LastFeedState = FeedState.Failed;
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException != null)
                    DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}: {3}", Name, Uri, ex.Message, ex.InnerException.Message);
                else
                    DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}", Name, Uri, ex.Message);

                this.LastFeedState = FeedState.Failed;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}", Name, Uri, ex.Message);
                this.LastFeedState = FeedState.Failed;
            }
        }




        #region Private helper functions

        void InitializeId(string uri)
        {
            Id = CryptoHelper.ComputeHashUsedByMobilizer(uri);
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

        static EntryWithPostProcessInfo Map(Entry o)
        {
            var result = new EntryWithPostProcessInfo();
            Copy(o, result);
            return result;
        }

        #endregion




        //void UpdateTeaserImage()
        //{
        //    TeaserImageUrl = News
        //        .Where(o => o.HasImage)
        //        .Select(o => o.GetBestImageUrl())
        //        .FirstOrDefault();
        //}

        public override string ToString()
        {
            return Name + ": " + Uri;
        }
    }
}
