
namespace System.Collections.Generic
{
    public class ModifiedTuple<T>
    {
        public T Original { get; private set; }
        public T Modified { get; private set; }

        internal ModifiedTuple(T original, T modified)
        {
            Original = original;
            Modified = modified;
        }
    }
}