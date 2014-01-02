using System;
using System.Collections.Generic;
using System.Linq;
using Weave.User.BusinessObjects;
using Weave.User.Paging.Lists;
using Weave.User.Paging.News;

namespace Weave.User.Paging
{
    /// <summary>
    /// Calculates the updated feeds, categories, and allnews for a given updated UserInfo
    /// </summary>
    public class PagedNewsHelper
    {
        public static PagedNewsHelper CalculatePagedNewsSince(UserInfo user, DateTime timeStamp, int pageSize = 50)
        {
            var helper = new PagedNewsHelper(user, timeStamp, pageSize);
            helper.Update();
            return helper;
        }




        #region Member variables

        readonly UserInfo user;
        readonly int pageSize;

        #endregion




        #region Public Readonly Properties

        public DateTime TimeStamp { get; private set; }
        public Guid ListId { get; private set; }
        public ListInfoByAll UpdatedAllNewsList { get; private set; }
        public List<ListInfoByCategory> UpdatedCategoryLists { get; private set; }
        public List<ListInfoByFeed> UpdatedFeedLists { get; private set; }

        #endregion




        #region Private constructor

        PagedNewsHelper(UserInfo user, DateTime timeStamp, int pageSize)
        {
            this.user = user;
            this.TimeStamp = timeStamp;
            this.pageSize = pageSize;

            UpdatedAllNewsList = new ListInfoByAll();
            UpdatedCategoryLists = new List<ListInfoByCategory>();
            UpdatedFeedLists = new List<ListInfoByFeed>();
        }

        #endregion




        #region Update function - does all the work of calculating the 3 lists

        void Update()
        {
            //TimeStamp = DateTime.UtcNow;
            ListId = Guid.NewGuid();

            var updatedFeeds = user.Feeds.Where(o => o.LastRefreshedOn > TimeStamp).ToList();

            if (updatedFeeds.Count == 0)
                return;

            var updatedCategories = updatedFeeds.Select(o => o.Category).Distinct().OfType<string>().ToList();
            var allFeeds = user.Feeds;

            UpdatedFeedLists.AddRange(updatedFeeds.Select(CreateListInfoForFeed));
            UpdatedCategoryLists.AddRange(updatedCategories.Select(CreateListInfoForCategory));
            UpdatedAllNewsList = CreateListInfoForAllNews();
        }

        #endregion





        #region Page Chunking functions

        ListInfoByAll CreateListInfoForAllNews()
        {
            var news = user.Feeds.AllOrderedNews().ToList();
            var pageCount = (int)Math.Ceiling((double)news.Count / (double)pageSize);

            return new ListInfoByAll
            {
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
                            new PagedNewsByAll
                            {
                                UserId = user.Id,
                                ListId = ListId,
                                Index = i,
                                NewsCount = o.Count,
                                News = o.Select(Convert).ToList(),
                            })
                        .ToList()
            };
        }

        ListInfoByCategory CreateListInfoForCategory(string category)
        {
            var news = user.Feeds.OfCategory(category).AllOrderedNews().ToList();
            var pageCount = (int)Math.Ceiling((double)news.Count / (double)pageSize);

            return new ListInfoByCategory
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
                        .ToList(),
            };
        }

        ListInfoByFeed CreateListInfoForFeed(Feed feed)
        {
            var news = feed.News.LatestNewsFirst().ToList();
            var pageCount = (int)Math.Ceiling((double)news.Count / (double)pageSize);

            return new ListInfoByFeed
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
