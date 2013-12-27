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

        /// <summary>
        /// A high-performance set comparison function for when we know both sets are ordered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsOrderedSetEqualTo<T>(this IOrderedEnumerable<T> source, IOrderedEnumerable<T> target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) return false;

            var sourceEnumerator = source.GetEnumerator();
            var targetEnumerator = target.GetEnumerator();

            while (true)
            {
                var sourceHasNext = sourceEnumerator.MoveNext();
                var targetHasNext = targetEnumerator.MoveNext();

                if (sourceHasNext != targetHasNext)
                    return false;

                if (!sourceHasNext)
                    return true;

                if (!sourceEnumerator.Current.Equals(targetEnumerator.Current))
                    return false;
            }
        }

        public static bool IsOrderedSetEqualTo<T>(this IOrderedEnumerable<T> source, IOrderedEnumerable<T> target, IEqualityComparer<T> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) return false;

            var sourceEnumerator = source.GetEnumerator();
            var targetEnumerator = target.GetEnumerator();

            while (true)
            {
                var sourceHasNext = sourceEnumerator.MoveNext();
                var targetHasNext = targetEnumerator.MoveNext();

                if (sourceHasNext != targetHasNext)
                    return false;

                if (!sourceHasNext)
                    return true;

                if (!comparer.Equals(sourceEnumerator.Current, targetEnumerator.Current))
                    return false;
            }
        }

        public static bool IsSetEqualTo<T>(this IEnumerable<T> source, IEnumerable<T> target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) return false;

            return !source.Except(target).Any() && !target.Except(source).Any();
        }

        public static bool IsSetEqualTo<T>(this IEnumerable<T> source, IEnumerable<T> target, IEqualityComparer<T> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null)  return false;

            return 
                !source.Except(target, comparer).Any() && 
                !target.Except(source, comparer).Any();
        }
    }
}
