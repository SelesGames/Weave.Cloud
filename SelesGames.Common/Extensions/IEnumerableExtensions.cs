using System.Linq;

namespace System.Collections.Generic
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Turns any IEnumerable sequence into a circularly wrapping infinite sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
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
        /// Computes the full set difference between two IEnumerable sequences.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="modified"></param>
        /// <returns></returns>
        public static SetDifference<T> Diff<T>(this IEnumerable<T> original, IEnumerable<T> modified)
        {
            if (original == null) throw new ArgumentNullException("original");
            if (modified == null) throw new ArgumentNullException("modified");

            var modifiedContents = original
                .Join(modified,
                    o => o,
                    o => o,
                    (o, m) => new ModifiedTuple<T>(o, m))
                .ToList();

            var addedContents = modified.Except(original).ToList();
            var removedContents = original.Except(modified).ToList();

            return new SetDifference<T>
            {
                Added = addedContents,
                Removed = removedContents,
                Modified = modifiedContents,
            };
        }

        /// <summary>
        /// Computes the full set difference between two IEnumerable sequences of different types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="modified"></param>
        /// <returns></returns>
        public static SetDifference<T1, T2> Diff<T1, T2, TCompare>(
            this IEnumerable<T1> original, 
            IEnumerable<T2> modified,
            Func<T1, TCompare> keySelector1,
            Func<T2, TCompare> keySelector2
            )
        {
            if (original == null) throw new ArgumentNullException("original");
            if (modified == null) throw new ArgumentNullException("modified");

            var modifiedContents = original
                .Join(modified,
                    o => keySelector1(o),
                    o => keySelector2(o),
                    (o, m) => Tuple.Create(o, m))
                .ToList();

            var originalTemp = original.Select(o => new KeyTemp<TCompare>(o, keySelector1(o))).ToList();
            var modifiedTemp = modified.Select(o => new KeyTemp<TCompare>(o, keySelector2(o))).ToList();

            var addedContents = modifiedTemp.Except(originalTemp).Select(o => (T2)o.ObjectRef).ToList();
            var removedContents = originalTemp.Except(modifiedTemp).Select(o => (T1)o.ObjectRef).ToList();

            return new SetDifference<T1, T2>
            {
                Added = addedContents,
                Removed = removedContents,
                Modified = modifiedContents,
            };
        }

        class KeyTemp<T>
        {
            public object ObjectRef { get; private set; }
            public T Key { get; private set; }

            public KeyTemp(object o, T key)
            {
                this.ObjectRef = o;
                this.Key = key;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is KeyTemp<T>))
                    return false;

                var key = ((KeyTemp<T>)obj).Key;

                return Key.Equals(key);
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }
        }
    }
}




#region deprecated set comparison functions

///// <summary>
///// A high-performance set comparison function for when we know both sets are ordered
///// </summary>
///// <typeparam name="T"></typeparam>
///// <param name="source"></param>
///// <param name="target"></param>
///// <returns></returns>
//public static bool IsOrderedSetEqualTo<T>(this IOrderedEnumerable<T> source, IOrderedEnumerable<T> target)
//{
//    if (source == null) throw new ArgumentNullException("source");
//    if (target == null) return false;

//    var sourceEnumerator = source.GetEnumerator();
//    var targetEnumerator = target.GetEnumerator();

//    while (true)
//    {
//        var sourceHasNext = sourceEnumerator.MoveNext();
//        var targetHasNext = targetEnumerator.MoveNext();

//        if (sourceHasNext != targetHasNext)
//            return false;

//        if (!sourceHasNext)
//            return true;

//        if (!sourceEnumerator.Current.Equals(targetEnumerator.Current))
//            return false;
//    }
//}

//public static bool IsOrderedSetEqualTo<T>(this IOrderedEnumerable<T> source, IOrderedEnumerable<T> target, IEqualityComparer<T> comparer)
//{
//    if (source == null) throw new ArgumentNullException("source");
//    if (target == null) return false;

//    var sourceEnumerator = source.GetEnumerator();
//    var targetEnumerator = target.GetEnumerator();

//    while (true)
//    {
//        var sourceHasNext = sourceEnumerator.MoveNext();
//        var targetHasNext = targetEnumerator.MoveNext();

//        if (sourceHasNext != targetHasNext)
//            return false;

//        if (!sourceHasNext)
//            return true;

//        if (!comparer.Equals(sourceEnumerator.Current, targetEnumerator.Current))
//            return false;
//    }
//}

//public static bool IsSetEqualTo<T>(this IEnumerable<T> source, IEnumerable<T> target)
//{
//    if (source == null) throw new ArgumentNullException("source");
//    if (target == null) return false;

//    return !source.Except(target).Any() && !target.Except(source).Any();
//}

//public static bool IsSetEqualTo<T>(this IEnumerable<T> source, IEnumerable<T> target, IEqualityComparer<T> comparer)
//{
//    if (source == null) throw new ArgumentNullException("source");
//    if (target == null)  return false;

//    return 
//        !source.Except(target, comparer).Any() && 
//        !target.Except(source, comparer).Any();
//}

#endregion