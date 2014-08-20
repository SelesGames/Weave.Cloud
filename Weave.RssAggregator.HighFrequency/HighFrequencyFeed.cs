using System;
using System.Collections.Generic;
using System.Linq;
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
        readonly Subject<HighFrequencyFeedUpdate> feedUpdate;

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyList<string> Instructions { get; private set; }

        public string Uri { get { return innerFeed.Uri; } }
        public IObservable<HighFrequencyFeedUpdate> FeedUpdate { get; private set; }

        public HighFrequencyFeed(string name, string feedUri, string originalUri, string instructions)
        {
            this.innerFeed = new Feed(feedUri, originalUri);
            this.feedUpdate = new Subject<HighFrequencyFeedUpdate>();

            Name = name;
            InitializeId(string.IsNullOrWhiteSpace(originalUri) ? feedUri : originalUri);

            FeedUpdate = feedUpdate.AsObservable();

            if (!string.IsNullOrWhiteSpace(instructions))
            {
                Instructions = instructions
                    .Split(',')
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o.Trim())
                    .ToList();
            }
        }

        /// <summary>
        /// Merge the feed's state (from Redis).  This will be used whenever the service restarts
        /// </summary>
        public void InitializeWith(Feed feed)
        {
            if (feed != null)
                feed.CopyStateTo(innerFeed);
        }

        public async Task Refresh()
        {
            var result = await innerFeed.Refresh();
            if (result != null)
            {
                var update = new HighFrequencyFeedUpdate(this, result);
                feedUpdate.OnNext(update);
            }
        }

        void InitializeId(string uri)
        {
            Id = SelesGames.Common.Hashing.CryptoHelper.ComputeHashUsedByMobilizer(uri);
        }

        public override string ToString()
        {
            return Name + ": " + Uri;
        }
    }
}