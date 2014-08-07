using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.HighFrequency.Processors.BestImageSelector;

namespace Weave.RssAggregator.HighFrequency
{
    public class BestImageSelectorProcessor : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(HighFrequencyFeedUpdateDto o)
        {
            if (o == null || EnumerableEx.IsNullOrEmpty(o.Entries))
                return;

            try
            {
                await Task.WhenAll(o.Entries.Select(ProcessEntry));
            }
            catch { }
        }

        async Task ProcessEntry(EntryWithPostProcessInfo e)
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
