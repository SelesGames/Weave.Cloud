using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeed
    {
        readonly Feed innerFeed;
        readonly Subject<FeedUpdate> feedUpdate;

        public Guid Id { get { return innerFeed.Id; } }
        public string Uri { get { return innerFeed.Uri; } }
        public IObservable<FeedUpdate> FeedUpdate { get; private set; }

        public HighFrequencyFeed(string name, string feedUri, string originalUri, string instructions)
        {
            this.innerFeed = new Feed(name, feedUri, originalUri, instructions);
            this.feedUpdate = new Subject<FeedUpdate>();
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