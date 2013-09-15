using System;

namespace Weave.User.Service.Cache.Extensions
{
    internal static class ObjectExtensions
    {
        public static T Cast<T>(this object o)
        {
            if (o == null)
                throw new ArgumentNullException("o");

            if (o is T)
                return (T)o;

            throw new InvalidCastException(string.Format("o is not of type {0}", typeof(T).Name));
        }
    }
}
