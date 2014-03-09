using Common.Caching;
using System.Threading.Tasks;
using Weave.Mobilizer.DTOs;

namespace Weave.Mobilizer.Cache
{
    public class LocalMemoryCache : LocalMemoryCache<string, Task<MobilizerResult>> { }
}
