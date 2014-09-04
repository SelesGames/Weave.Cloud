using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.HighFrequency.Processors.BestImageSelector;
using Weave.Updater.BusinessObjects;

namespace Weave.RssAggregator.HighFrequency
{
    public class BestImageSelectorProcessor : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        public async Task ProcessAsync(HighFrequencyFeedUpdate o)
        {
            if (o == null || EnumerableEx.IsNullOrEmpty(o.Entries))
                return;

            try
            {
                await Task.WhenAll(o.Entries.Select(ProcessEntry));

                var feed = o.InnerFeed;
                var update = o.InnerUpdate;

                // fix the bool flag for whether the NewsItemRecords have images
                var joined = update.Entries.Join(feed.News, x => x.Id, x => x.Id, (e, n) => new { e, n });
                foreach (var tuple in joined)
                {
                    var record = tuple.n;
                    var entry = tuple.e;

                    record.HasImage = entry.Images.Any();
                }
            }
            catch { }
        }

        async Task ProcessEntry(ExpandedEntry e)
        {
            try
            {
                var helper = new BestImageSelectorHelper(e);
                await helper.Process();
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }
        }
    }
}
