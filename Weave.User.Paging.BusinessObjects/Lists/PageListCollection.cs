using System;
using System.Collections.Generic;
using System.Linq;
using Weave.User.BusinessObjects;

namespace Weave.User.Paging.BusinessObjects.Lists
{
    public class PageListCollection
    {
        Dictionary<string, CategoryPageList> categoryListLookup;
        Dictionary<Guid, FeedPageList> feedListLookup;

        public Guid UserId { get; set; }

        public IEnumerable<CategoryPageList> CategoryLists
        {
            get { return categoryListLookup.Select(o => o.Value); }
        }

        public IEnumerable<FeedPageList> FeedLists
        {
            get { return feedListLookup.Select(o => o.Value); }
        }

        public PageListCollection(
            IEnumerable<CategoryPageList> categories, 
            IEnumerable<FeedPageList> feeds)
        {
            categoryListLookup = categories == null ? 
                new Dictionary<string, CategoryPageList>() : categories.ToDictionary(o => o.Category);

            feedListLookup = feeds == null ?
                new Dictionary<Guid, FeedPageList>() : feeds.ToDictionary(o => o.FeedId);
        }

        public DateTime? GetLatestRefreshForFeed(Feed feed)
        {
            FeedPageList feedList;
            if (feedListLookup.TryGetValue(feed.Id, out feedList))
            {
                return feedList.LatestRefresh;
            }
            return null;
        }

        public DateTime? GetLatestRefreshForCategory(string category)
        {
            CategoryPageList categoryList;
            if (categoryListLookup.TryGetValue(category, out categoryList))
            {
                return categoryList.LatestRefresh;
            }
            return null;
        }

        public CategoryPageList Get(string category)
        {
            if (categoryListLookup.ContainsKey(category))
                return categoryListLookup[category];
            else
                return null;
        }

        public FeedPageList Get(Guid feedId)
        {
            if (feedListLookup.ContainsKey(feedId))
                return feedListLookup[feedId];
            else
                return null;
        }

        public void Add(CategoryListInfo list)
        {
            var pageList = Get(list.Category);
            if (pageList != null)
                pageList.Add(list);
        }

        public void Add(FeedListInfo list)
        {
            var pageList = Get(list.FeedId);
            if (pageList != null)
                pageList.Add(list);
        }
    }
}
