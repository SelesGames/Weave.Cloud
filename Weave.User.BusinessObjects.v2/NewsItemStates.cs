using System;
using System.Collections.Generic;

namespace Weave.User.BusinessObjects.v2
{
    public class NewsItemStates : Dictionary<string, byte>
    {
        public static NewsItemStates Create(IEnumerable<Tuple<string, bool, bool>> states)
        {
            var result = new NewsItemStates();

            foreach (var tuple in states)
            {
                var id = tuple.Item1;
                var isRead = tuple.Item2;
                var isFavorite = tuple.Item3;

                var state = NewsItemState.None;

                if (isRead)
                    state = state | NewsItemState.Read;

                if (isFavorite)
                    state = state | NewsItemState.Favorite;

                result.Add(id, (byte)state);
            }

            return result;
        }

        public void MarkRead(string newsItemId)
        {
            this.AddOrUpdate(newsItemId, (byte)NewsItemState.Read, 
                (key, val) => Set(val, NewsItemState.Read));
        }

        public void MarkUnread(string newsItemId)
        {
            this.AddOrUpdate(newsItemId, (byte)NewsItemState.None,
                (key, val) => Remove(val, NewsItemState.Read));
        }

        public void AddFavorite(string newsItemId)
        {
            this.AddOrUpdate(newsItemId, (byte)NewsItemState.Favorite,
                (key, val) => Set(val, NewsItemState.Favorite));
        }

        public void RemoveFavorite(string newsItemId)
        {
            this.AddOrUpdate(newsItemId, (byte)NewsItemState.None,
                (key, val) => Remove(val, NewsItemState.Favorite));
        }




        #region helper methods

        byte Set(byte b, NewsItemState state)
        {
            var currentState = (NewsItemState)b;
            currentState = currentState | state;
            return (byte)currentState;
        }

        byte Remove(byte b, NewsItemState state)
        {
            var currentState = (NewsItemState)b;
            currentState = currentState & (~state);
            return (byte)currentState;
        }

        #endregion
    }

    public static class DictionaryExtensions
    {
        public static TValue AddOrUpdate<TKey, TValue>(
            this Dictionary<TKey, TValue> dict, 
            TKey key, 
            TValue addValue, 
            Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (dict.ContainsKey(key))
            {
                var existing = dict[key];
                var updated = updateValueFactory(key, existing);
                dict[key] = updated;
                return updated;
            }
            else
            {
                dict[key] = addValue;
                return addValue;
            }
        }
    }
}
