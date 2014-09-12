using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.Updater.BusinessObjects;
using Weave.User.Service.Redis;

namespace Weave.RssAggregator.HighFrequency
{
    public class HighFrequencyFeed
    {
        readonly Feed innerFeed;

        public string Name { get; private set; }
        public string Uri { get { return innerFeed.Uri; } }
        public IEnumerable<string> Instructions { get; private set; }
        public IEnumerable<string> PreviousUris { get; private set; }

        public HighFrequencyFeed(string name, string uri, string instructions, IEnumerable<string> previousUris = null)
        {
            this.innerFeed = new Feed(uri);

            Name = name;

            if (!string.IsNullOrWhiteSpace(instructions))
            {
                Instructions = instructions
                    .Split(',')
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => o.Trim())
                    .ToList();
            }
            else
                Instructions = new List<string>(0);

            PreviousUris = previousUris ?? new List<string>();
        }

        /// <summary>
        /// Merge the feed's state (from Redis).  This will be used whenever the service restarts
        /// </summary>
        public void InitializeWith(Feed feed)
        {
            if (feed != null)
                feed.CopyStateTo(innerFeed);
        }

        public async Task Refresh(IAsyncProcessor<HighFrequencyFeedUpdate> processor)
        {
            var result = await innerFeed.Refresh();
            if (result != null)
            {
                var update = new HighFrequencyFeedUpdate(this, result);
                await processor.ProcessAsync(update);
            }
        }

        public override string ToString()
        {
            return Name + ": " + Uri;
        }
    }
}