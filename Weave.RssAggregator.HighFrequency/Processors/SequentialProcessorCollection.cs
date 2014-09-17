using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weave.FeedUpdater.HighFrequency
{
    public class SequentialProcessorCollection<T> : IAsyncProcessor<T>
    {
        readonly IEnumerable<IAsyncProcessor<T>> processors;

        public SequentialProcessorCollection(IEnumerable<IAsyncProcessor<T>> processors)
        {
            this.processors = processors;
        }

        public async Task ProcessAsync(T o)
        {
            foreach (var processor in processors)
            {
                try
                {
                    await processor.ProcessAsync(o);
                }
                catch (Exception e)
                {
                    DebugEx.WriteLine("\r\n\r\n**** PROCESSOR {0} ERROR ****", processor.GetType().Name);
                    DebugEx.WriteLine(e);

                    //DebugEx.WriteLine(e);
                    //throw e;
                }
            }
        }
    }
}
