
namespace Weave.Mobilizer.Core.Cache
{
    public interface IBasicCache<TKey, TResult>
    {
        TResult Get(TKey key);
    }
}
