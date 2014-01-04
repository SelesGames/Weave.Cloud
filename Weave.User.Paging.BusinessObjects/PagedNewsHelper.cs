using System;
using System.Collections.Generic;
using System.Linq;
using Weave.User.BusinessObjects;
using Weave.User.Paging.BusinessObjects.Lists;
using Weave.User.Paging.BusinessObjects.News;

namespace Weave.User.Paging.BusinessObjects
{
    /// <summary>
    /// Calculates the updated feeds, categories, and allnews for a given updated UserInfo
    /// </summary>
    public class PagedNewsHelper
    {
        public static PagedNewsHelper CalculatePagedNewsSince(UserInfo user, PageListCollection masterList, int pageSize = 50)
        {
            var helper = new PagedNewsHelper(user, masterList, pageSize);
            helper.Update();
            return helper;
        }




        #region Member variables

        readonly UserInfo user;
        readonly PageListCollection masterList;
        readonly int pageSize;

        #endregion




        #region Public Readonly Properties

        public DateTime TimeStamp { get; private set; }
        public Guid ListId { get; private set; }
        //public List<CategoryListInfo> UpdatedCategoryLists { get; private set; }
        //public List<FeedListInfo> UpdatedFeedLists { get; private set; }
        public List<ListInfo> UpdatedLists { get; private set; }

        #endregion




        #region Private constructor

        PagedNewsHelper(UserInfo user, PageListCollection masterList, int pageSize)
        {
            this.user = user;
            this.masterList = masterList;
            this.pageSize = pageSize;

            //UpdatedCategoryLists = new List<CategoryListInfo>();
            //UpdatedFeedLists = new List<FeedListInfo>();
            UpdatedLists = new List<ListInfo>();
        }

        #endregion




        #region Update function - does all the work of calculating the 3 lists

        void Update()
        {
            TimeStamp = DateTime.UtcNow;
            ListId = Guid.NewGuid();

            var updatedFeeds = user.Feeds
                .Where(o => o.News != null)
                .Where(FeedShouldBeIncluded)
                .ToList();

            if (updatedFeeds.Count == 0)
                return;

            //var updatedCategories = new List<string> { "all news" };
            //updatedCategories.AddRange(updatedFeeds.Select(o => o.Category).Distinct().OfType<string>());

            var updatedCategories = updatedFeeds.Select(o => o.Category).Distinct().OfType<string>();
            var allFeeds = user.Feeds;

            //UpdatedFeedLists.AddRange(updatedFeeds.Select(CreateListInfoForFeed));
            //UpdatedCategoryLists.AddRange(updatedCategories.Select(CreateListInfoForCategory));
            UpdatedLists.AddRange(updatedFeeds.Select(CreateListInfoForFeed));
            UpdatedLists.AddRange(updatedCategories.Select(CreateListInfoForCategory));
        }

        bool FeedShouldBeIncluded(Feed feed)
        {
            var latestRefresh = masterList.GetLatestRefreshForFeed(feed);
            return latestRefresh == null || feed.LastRefreshedOn > latestRefresh.Value;
        }

        #endregion




        #region Page Chunking functions

        CategoryListInfo CreateListInfoForCategory(string category)
        {
            var news = user.Feeds.OfCategory(category).AllOrderedNews().ToList();
            var pageCount = (int)Math.Ceiling((double)news.Count / (double)pageSize);

            return new CategoryListInfo
            {
                Category = category,
                ListId = ListId,
                CreatedOn = TimeStamp,
                LastAccess = null,
                PageSize = pageSize,
                PageCount = pageCount,
                PagedNews = 
                    Enumerable
                        .Range(0, pageCount)
                        .Select(i => news.Skip(i * pageSize).Take(pageSize).ToList())
                        .Select((o, i) =>
                            new PagedNewsByCategory
                            {
                                UserId = user.Id,
                                Category = category,
                                ListId = ListId,
                                Index = i,
                                NewsCount = o.Count,
                                News = o.Select(Convert).ToList(),
                            })
                        .OfType<PagedNewsBase>()
                        .ToList(),
            };
        }

        FeedListInfo CreateListInfoForFeed(Feed feed)
        {
            var news = new[] { feed }.AllOrderedNews().ToList();
            var pageCount = (int)Math.Ceiling((double)news.Count / (double)pageSize);

            return new FeedListInfo
            {
                FeedId = feed.Id,
                ListId = ListId,
                CreatedOn = TimeStamp,
                LastAccess = null,
                PageSize = pageSize,
                PageCount = pageCount,
                PagedNews =
                    Enumerable
                        .Range(0, pageCount)
                        .Select(i => news.Skip(i * pageSize).Take(pageSize).ToList())
                        .Select((o, i) =>
                            new PagedNewsByFeed
                            {
                                UserId = user.Id,
                                FeedId = feed.Id,
                                ListId = ListId,
                                Index = i,
                                NewsCount = o.Count,
                                News = o.Select(Convert).ToList(),
                            })
                        .OfType<PagedNewsBase>()
                        .ToList()
            };
        }

        #endregion




        #region Conversion helpers

        Store.News.NewsItem Convert(NewsItem o)
        {
            return new Store.News.NewsItem
            {
                Id = o.Id,
                FeedId = o.Feed.Id,
                Title = o.Title,
                Link = o.Link,
                UtcPublishDateTime = o.UtcPublishDateTimeString,
                ImageUrl = o.ImageUrl,
                YoutubeId = o.YoutubeId,
                VideoUri = o.VideoUri,
                PodcastUri = o.PodcastUri,
                ZuneAppId = o.ZuneAppId,
                OriginalDownloadDateTime = o.OriginalDownloadDateTime,
                Image = o.Image == null ? null :
                    new Store.News.Image
                    {
                        Width = o.Image.Width,
                        Height = o.Image.Height,
                        OriginalUrl = o.Image.OriginalUrl,
                        BaseImageUrl = o.Image.BaseImageUrl,
                        SupportedFormats = o.Image.SupportedFormats,
                    },
            };
        }

        #endregion
    }
}
