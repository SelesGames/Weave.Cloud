
namespace System.Collections.Generic
{
    public class SetDifference<T>
    {
        internal SetDifference() { }

        public IEnumerable<T> Added { get; set; }
        public IEnumerable<T> Removed { get; set; }
        public IEnumerable<ModifiedTuple<T>> Modified { get; set; }
    }

    public class SetDifference<T1, T2>
    {
        internal SetDifference() { }

        public IEnumerable<T2> Added { get; set; }
        public IEnumerable<T1> Removed { get; set; }
        public IEnumerable<Tuple<T1, T2>> Modified { get; set; }
    }
}