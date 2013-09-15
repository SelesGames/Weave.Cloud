using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects
{
    public class UserNewsItemState
    {
        Dictionary<Guid, NewsItemState> dict;

        public Guid Id { get; private set; }

        public UserNewsItemState(Guid id, IEnumerable<NewsItemState> newsItemStates)
        {
            Id = id;
            if (newsItemStates != null)
            {
                dict = newsItemStates.ToDictionary(o => o.Id);
            }
            else
            {
                dict = new Dictionary<Guid, NewsItemState>();
            }
        }

        void ChangeState(Guid newsItemId, Action<NewsItemState> modifyAction)
        {
            NewsItemState state;

            if (dict.ContainsKey(newsItemId))
            {
                state = dict[newsItemId];
            }
            else
            {
                state = new NewsItemState { Id = newsItemId };
                dict.Add(newsItemId, state);
            }

            modifyAction(state);

            // if the article is neither viewed nor favorited, 
            // then it has the "default" state and doesn't need to be tracked
            if (state.IsFavorite == false && state.HasBeenViewed == false)
            {
                dict.Remove(newsItemId);
            }
        }

        public void MarkRead(params Guid[] newsItemIds)
        {
            if (newsItemIds == null)
                return;

            foreach (var newsItemId in newsItemIds)
            {
                var id = newsItemId;
                ChangeState(id, state => state.HasBeenViewed = true);
            }
        }

        //public void MarkRead(Guid newsItemId)
        //{
        //    ChangeState(newsItemId, state => state.HasBeenViewed = true);
        //}

        public void MarkUnread(Guid newsItemId)
        {
            ChangeState(newsItemId, state => state.HasBeenViewed = false);
        }

        public void Favorite(Guid newsItemId)
        {
            ChangeState(newsItemId, state => state.IsFavorite = true);
        }

        public void Unfavorite(Guid newsItemId)
        {
            ChangeState(newsItemId, state => state.IsFavorite = false);
        }

        public List<NewsItemState> GetNewsItemStatesList()
        {
            return dict.Select(o => o.Value).ToList();
        }

        public void SetNewsState(UserInfo user)
        {
            if (user.Feeds == null)
                return;

            foreach (var newsItem in user.Feeds.AllNews())
            {
                var key = newsItem.Id;
                if (dict.ContainsKey(key))
                {
                    var state = dict[key];
                    newsItem.HasBeenViewed = state.HasBeenViewed;
                    newsItem.IsFavorite = state.IsFavorite;
                }
            }
        }
    }
}
