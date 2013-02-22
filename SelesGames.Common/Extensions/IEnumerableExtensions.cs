using System.Linq;

namespace System.Collections.Generic
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Wrap<T>(this IEnumerable<T> o)
        {
            if (o == null)
                throw new ArgumentNullException("parameter in IEnumerableExtensions.Wrap");

            if (!o.Any())
                yield break;

            var enumerator = o.GetEnumerator();

            while (enumerator != null)
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
                enumerator = o.GetEnumerator();
            }
        }
    }
}
