using StackExchange.Redis;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeed
    {
        readonly Feed innerFeed;
        readonly Subject<FeedUpdate> feedUpdate;
        readonly IDatabaseAsync db;

        public Guid Id { get { return innerFeed.Id; } }
        public string Uri { get { return innerFeed.Uri; } }
        public IObservable<FeedUpdate> FeedUpdate { get; private set; }

        public HighFrequencyFeed(string name, string feedUri, string originalUri, string instructions, IDatabaseAsync db)
        {
            this.innerFeed = new Feed(name, feedUri, originalUri, instructions);
            this.feedUpdate = new Subject<FeedUpdate>();
            this.db = db;
            FeedUpdate = feedUpdate.AsObservable();
        }

        /// <summary>
        /// Recover the feed's state from Redis.  this will be used whenever the service restarts
        /// </summary>
        public async Task Initialize()
        {
            var cache = new FeedUpdaterCache(db);

            var cachedDataResult = await cache.Get(Id);
            if (cachedDataResult.HasValue)
            {
                var cachedData = cachedDataResult.Value;
                CopyState(cachedData);
            }
        }

        void CopyState(Feed o)
        {
            innerFeed.TeaserImageUrl = o.TeaserImageUrl;
            innerFeed.LastRefreshedOn = o.LastRefreshedOn;
            innerFeed.Etag = o.Etag;
            innerFeed.LastModified = o.LastModified;
            innerFeed.MostRecentNewsItemPubDate = o.MostRecentNewsItemPubDate;

            foreach (var entry in o.Entries)
                innerFeed.Entries.Add(entry);
        }

        public async Task Refresh()
        {
            var result = await innerFeed.Refresh();
            if (result != null)
                feedUpdate.OnNext(result);
        }

        public override string ToString()
        {
            return innerFeed.Name + ": " + innerFeed.Uri;
        }
    }
}