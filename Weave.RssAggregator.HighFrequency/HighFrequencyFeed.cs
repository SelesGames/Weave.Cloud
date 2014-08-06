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


        public Guid FeedId { get; private set; }
        public string Name { get; private set; }
        public string FeedUri { get; private set; }
        public string Etag { get; private set; }
        public string LastModified { get; private set; }
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
            FeedUri = feedUri;
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
                    FeedId = this.FeedId,
                    FeedUri = this.FeedUri,
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
                        var addedNews = new List<Entry>();

                        foreach (var o in requester.News)
                        {
                            if (News.Add(o))
                                addedNews.Add(o);
                        }

                        News.TrimTo(NUMBER_OF_NEWSITEMS_TO_HOLD);

                        if (addedNews.Any())
                        {
                            var update = new HighFrequencyFeedUpdateDto
                            {
                                Feed = this,
                                FeedId = FeedId,
                                Name = Name,
                                FeedUri = FeedUri,
                                RefreshTime = refreshTime,
                                Instructions = instructions,
                                Entries = addedNews.Select(Map).ToList(),
                            };
                            feedUpdate.OnNext(update);
                        }
                    }

                    DebugEx.WriteLine("REFRESHED {0}  ({1})", Name, FeedUri);
                }
                else if (result == Feed.RequestStatus.Unmodified)
                {
                    DebugEx.WriteLine("UNMODIFIED {0}  ({1})", Name, FeedUri);
                }
                this.LastFeedState = FeedState.OK;
            }
            catch (TaskCanceledException ex)
            {
                DebugEx.WriteLine("!!!!!! TIMED OUT {0}  ({1}): {2}", Name, FeedUri, ex.Message);
                this.LastFeedState = FeedState.Failed;
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException != null)
                    DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}: {3}", Name, FeedUri, ex.Message, ex.InnerException.Message);
                else
                    DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}", Name, FeedUri, ex.Message);

                this.LastFeedState = FeedState.Failed;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine("!!!!!! FAILED {0}  ({1}): {2}", Name, FeedUri, ex.Message);
                this.LastFeedState = FeedState.Failed;
            }
        }




        #region Private helper functions

        void InitializeId(string uri)
        {
            FeedId = CryptoHelper.ComputeHashUsedByMobilizer(uri);
        }

        #endregion




        #region Map functions

        static EntryWithPostProcessInfo Map(Entry o)
        {
            var result = new EntryWithPostProcessInfo
            {
                Id = o.Id,
                FeedId = o.FeedId,
                UtcPublishDateTime = o.UtcPublishDateTime,
                Title = o.Title,
                OriginalPublishDateTimeString = o.OriginalPublishDateTimeString,
                Link = o.Link,
                Description = o.Description,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                OriginalRssXml = o.OriginalRssXml,
            };

            result.Image.OriginalUrl = o.GetImageUrl();
            return result;
        }

        #endregion




        public override string ToString()
        {
            return Name + ": " + FeedUri;
        }
    }
}
