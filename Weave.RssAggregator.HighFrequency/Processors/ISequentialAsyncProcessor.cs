using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public interface ISequentialAsyncProcessor<T>
    {
        bool IsHandledFully { get; }
        Task ProcessAsync(T o);
    }
}
