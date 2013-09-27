using System.Linq;

namespace System.Collections.Generic
{
    internal static class EnumerableEx
    {
        public static bool IsNullOrEmpty<T>(IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
    }
}
