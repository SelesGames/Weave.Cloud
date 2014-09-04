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
