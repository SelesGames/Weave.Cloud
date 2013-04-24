using System.Linq;

namespace System.Collections.Generic
{
    public static class EnumerableEx
    {
        public static bool IsNullOrEmpty<T>(IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
    }
}
