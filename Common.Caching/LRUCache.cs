using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Common.Caching
{
    /// <summary>
    /// Implementation of a Least Recently Used Cache
    /// </summary>
    public class LRUCache<TKey, TValue> : IEnumerable<LRUCacheItem<TKey, TValue>>, IEnumerable
    {
        readonly int capacity;
        readonly object sync = new object();

        readonly LinkedList<LRUCacheItem<TKey, TValue>> list;
        readonly Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>> cache;

        public LRUCache(int capacity)
        {
            if (capacity < 0) throw new ArgumentException("capacity");

            this.capacity = capacity;
            list = new LinkedList<LRUCacheItem<TKey, TValue>>();
            cache = new Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>>(capacity + 1);
        }

        TValue Get(TKey key)
        {
            lock (sync)
            {
                LinkedListNode<LRUCacheItem<TKey, TValue>> node;

                if (!cache.TryGetValue(key, out node))
                    return default(TValue);

                list.Remove(node);
                list.AddLast(node);

                return node.Value.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="checkPresenceOfKeyBeforeAdd"></param>
        /// <returns>The evicted LRU item, if any</returns>
        LRUCacheItem<TKey, TValue> AddOrUpdate(TKey key, TValue value, bool checkPresenceOfKeyBeforeAdd)
        {
            LRUCacheItem<TKey, TValue> evicted = null;

            lock (sync)
            {
                LinkedListNode<LRUCacheItem<TKey, TValue>> node;

                // if the cache already contains an entry for the key, update it's value and move it to the front of the list
                if (cache.TryGetValue(key, out node))
                {
                    list.Remove(node);
                    list.AddLast(node);

                    node.Value.Value = value;
                }
                // cache does not contain a value for key, so add it now
                else
                {
                    var cacheItem = new LRUCacheItem<TKey, TValue>(key, value);
                    node = new LinkedListNode<LRUCacheItem<TKey, TValue>>(cacheItem);

                    cache.Add(key, node);
                    list.AddLast(node);
                }

                if (cache.Count > capacity)
                    evicted = EvictLRU();
            }

            return evicted;
        }

        /// <summary>
        /// Evicts the least recently used cache item
        /// </summary>
        /// <returns>Returns the value of the item that was evicted</returns>
        LRUCacheItem<TKey, TValue> EvictLRU()
        {
            var lru = list.First;

            cache.Remove(lru.Value.Key);
            list.Remove(lru);

            return lru.Value;
        }




        #region IEnumerable  implementation

        public IEnumerator<LRUCacheItem<TKey, TValue>> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion




        #region Partial Collection implementation

        public int Count
        {
            get { return list.Count; }
        }

        public void Clear()
        {
            lock (sync)
            {
                list.Clear();
                cache.Clear();
            }
        }

        #endregion




        #region Partical IDictionary<TKey, TValue> implementation

        public ICollection<TKey> Keys
        {
            get { return cache.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return cache.Values.Select(o => o.Value.Value).ToList(); }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>
        /// The value associated with the specified key. If the specified key is not
        /// found, a get operation throws a System.Collections.Generic.KeyNotFoundException,
        /// and a set operation creates a new element with the specified key.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException:">The property is retrieved and key does not exist in the collection.</exception>
        public TValue this[TKey key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                AddOrUpdate(key, value, true);
            }
        }

        // Summary:
        //     Adds an element with the provided key and value to the LRUCache<TKey,TValue>.
        //
        // Parameters:
        //   key:
        //     The object to use as the key of the element to add.
        //
        //   value:
        //     The object to use as the value of the element to add.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //
        //   System.ArgumentException:
        //     An element with the same key already exists in the System.Collections.Generic.IDictionary<TKey,TValue>.
        public void Add(TKey key, TValue value)
        {
            AddOrUpdate(key, value, false);
        }

        // Summary:
        //     Determines whether the LRUCache<TKey,TValue>
        //     contains an element with the specified key.
        //
        // Parameters:
        //   key:
        //     The key to locate in the LRUCache<TKey,TValue>.
        //
        // Returns:
        //     true if the LRUCache<TKey,TValue> contains
        //     an element with the key; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        public bool ContainsKey(TKey key)
        {
            return cache.ContainsKey(key);
        }

        /// <summary>
        /// Removes the element with the specified key from the LRUCache&lt;TKey,TValue&gt;.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false. This method
        /// also returns false if key was not found in the original LRUCacheLRUCache&lt;TKey,TValue&gt;.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// key is null.
        /// </exception>
        public bool Remove(TKey key)
        {
            lock (sync)
            {
                if (cache.ContainsKey(key))
                {
                    var node = cache[key];

                    list.Remove(node);
                    return cache.Remove(key);
                }
                else
                    return false;
            }
        }

        #endregion
    }
}
