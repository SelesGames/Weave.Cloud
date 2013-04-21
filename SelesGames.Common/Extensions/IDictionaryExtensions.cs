
namespace System.Collections.Generic
{
    public static class IDictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            TValue temp;

            return dict.TryGetValue(key, out temp) ? temp : default(TValue);
        }
    }
}
