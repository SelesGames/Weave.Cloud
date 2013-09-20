using SelesGames.Common;
using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Weave.Parsing;
using Weave.RssAggregator.Client;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeed
    {
        Subject<HighFrequencyFeedUpdateDto> feedUpdate = new Subject<HighFrequencyFeedUpdateDto>();
        List<Guid> lastNewsIds;
        IReadOnlyList<string> instructions;


        public Guid FeedId { get; private set; }
        public string Name { get; private set; }
        public string FeedUri { get; private set; }
        public string Etag { get; private set; }
        public string LastModified { get; private set; }
        public TimeSpan RefreshTimeout { get; set; }
        public FeedState LastFeedState { get; private set; }

        public IObservable<HighFrequencyFeedUpdateDto> FeedUpdate { get; private set; }


        public enum FeedState
        {
            Uninitialized,
            Failed,
            OK
        }


        public HighFrequencyFeed(string name, string feedUri, string instructions)
        {
            if (string.IsNullOrWhiteSpace(name))        throw new ArgumentException("name in HighFrequencyFeed ctor");
            if (string.IsNullOrWhiteSpace(feedUri))     throw new ArgumentException("name in HighFrequencyFeed ctor");

            Name = name;
            FeedUri = feedUri;
            InitializeId();
            LastFeedState = FeedState.Uninitialized;
            FeedUpdate = feedUpdate.AsObservable();
            RefreshTimeout = TimeSpan.FromMinutes(1);

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
                var result = await requester.UpdateFeed();

                if (result == Feed.RequestStatus.OK)
                {
                    this.Etag = requester.Etag;
                    this.LastModified = requester.LastModified;

                    var news = requester.News;

                    if (news != null && news.Any())
                    {
                        if (IsNewsNew(news))
                        {
                            lastNewsIds = news.Select(o => o.Id).ToList();

                            var update = new HighFrequencyFeedUpdateDto
                            {
                                FeedId = FeedId,
                                Name = Name,
                                FeedUri = FeedUri,
                                RefreshTime = refreshTime,
                                Instructions = instructions,
                                Entries = news.Select(o => o.Convert(EntryToEntryWithPostProcessInfoConverter.Instance)).ToList(),
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

        void InitializeId()
        {
            FeedId = CryptoHelper.ComputeHashUsedByMobilizer(FeedUri);
        }

        bool IsNewsNew(List<Entry> news)
        {
            if (lastNewsIds == null)
                return true;

            if (news.Count != lastNewsIds.Count)
                return true;

            foreach (var idTuple in lastNewsIds.Zip(news, (i, j) => new { Id1 = i, Id2 = j.Id }))
            {
                if (!idTuple.Id1.Equals(idTuple.Id2))
                    return true;
            }

            return false;
        }

        #endregion




        public override string ToString()
        {
            return Name + ": " + FeedUri;
        }
    }
}
