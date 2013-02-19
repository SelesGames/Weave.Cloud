using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Weave.RssAggregator.HighFrequency
{
    public abstract class ThreadSafeDelegatingCollection<TKey, T> : ICollection<T>
    {
        ConcurrentDictionary<TKey, T> lookup;

        protected abstract void AddInternal(T item);
        protected abstract void RemoveInternal(T item);
        protected abstract TKey GetKey(T item);

        public void Add(T item)
        {
            var key = GetKey(item);

            lookup.AddOrUpdate(key, item, (k, i) => i);
        }

        public void Clear()
        {
            lookup.Clear();
        }

        public bool Contains(T item)
        {
            var key = GetKey(item);

            return lookup.ContainsKey(key);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return lookup.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            T val;
            var key = GetKey(item);

            return lookup.TryRemove(key, out val);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return lookup.Select(o => o.Value).GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return lookup.Select(o => o.Value).GetEnumerator();
        }
    }
}
