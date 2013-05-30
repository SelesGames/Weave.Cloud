using Common.Caching;
using System.Threading.Tasks;
using Weave.Readability;

namespace Weave.Mobilizer.Core.Cache
{
    public class LocalMemoryCache : LocalMemoryCache<string, Task<ReadabilityResult>> { }
}
