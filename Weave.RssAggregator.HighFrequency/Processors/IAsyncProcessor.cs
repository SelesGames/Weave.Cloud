using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public interface IAsyncProcessor<T>
    {
        Task ProcessAsync(T o);
    }
}