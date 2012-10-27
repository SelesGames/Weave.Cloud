using System;

namespace Weave.Mobilizer.Core.Cache
{
    public interface IExtendedCache<TKey, TResult>
    {
        TResult GetOrAdd(TKey key, Func<TKey, TResult> valueFactory);
    }
}
