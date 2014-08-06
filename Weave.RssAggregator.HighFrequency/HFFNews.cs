using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Weave.Parsing;

namespace Weave.RssAggregator.HighFrequency
{
    public class HFFNews : IEnumerable<Entry>
    {
        static readonly EntryComparer entryComparer = new EntryComparer();

        SortedSet<Entry> set;

        public HFFNews()
        {
            set = new SortedSet<Entry>(entryComparer);
        }

        /// <summary>
        /// Adds an Entry to the High Frequency Feed's News collection
        /// </summary>
        /// <param name="feed">The Entry to be added</param>
        /// <returns>True if the entry was added, false if the entry was already present or invalid</returns>
        public bool Add(Entry entry)
        {
            if (entry == null) return false;
            if (string.IsNullOrWhiteSpace(entry.Link) ||
                string.IsNullOrWhiteSpace(entry.Title))
                return false;

            if (entry.Id == Guid.Empty)
                return false;

            return set.Add(entry);
        }

        public void TrimTo(int target)
        {
            set = new SortedSet<Entry>(set.Take(target), entryComparer);
        }




        #region helper class for doing the Entry comparisons

        class EntryComparer : IComparer<Entry>
        {
            public int Compare(Entry x, Entry y)
            {
                if (x.Id == y.Id)
                    return 0;

                if (x.Title.Equals(y.Title, StringComparison.OrdinalIgnoreCase))
                    return 0;

                if (x.Link.Equals(y.Link, StringComparison.OrdinalIgnoreCase))
                    return 0;

                if (x.UtcPublishDateTime <= y.UtcPublishDateTime)
                    return -1;
                else
                    return 1;
            }
        }

        #endregion




        #region IEnumerable interface implementation

        public IEnumerator<Entry> GetEnumerator()
        {
            return set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return set.GetEnumerator();
        }

        #endregion
    }
}
