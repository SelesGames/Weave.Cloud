using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;
using Weave.User.Paging.Lists;
using Weave.User.Paging.News;

namespace Weave.User.Paging
{
    public class PagedNewsHelper
    {
        UserInfo user;
        int pageSize;

        // volatile, figure out better approach
        DateTime now;
        Guid listId;
        ListInfoByAll listInfoByAllNews;
        List<ListInfoByCategory> listInfoByCategory;
        List<ListInfoByFeed> listInfoByFeed;

        MasterListsInfo masterList;

        public PagedNewsHelper(UserInfo user, int pageSize = 50)
        {
            this.user = user;
            this.pageSize = pageSize;

            listInfoByCategory = new List<ListInfoByCategory>();
            listInfoByFeed = new List<ListInfoByFeed>();

            masterList = new MasterListsInfo();
        }

        public async Task Update()
        {
            now = DateTime.UtcNow;
            listId = Guid.NewGuid();

            await user.RefreshAllFeeds();

            var updatedFeeds = user.Feeds.Where(o => o.LastRefreshedOn > now).ToList();

            if (updatedFeeds.Count == 0)
                return;

            var updatedCategories = updatedFeeds.Select(o => o.Category).Distinct().ToList();
            var allFeeds = user.Feeds;

            listInfoByFeed.AddRange(updatedFeeds.Select(CreateListInfoForFeed));
            listInfoByCategory.AddRange(updatedCategories.Select(CreateListInfoForCategory));
            listInfoByAllNews = CreateListInfoForAllNews();

            masterList.AllNewsLists.Add(listInfoByAllNews);
            masterList.CategoryLists.AddRange(listInfoByCategory);
            masterList.FeedLists.AddRange(listInfoByFeed);

            var pagedNews = new IEnumerable<PagedNewsBase>[] 
            {
                listInfoByAllNews.PagedNews, 
                listInfoByCategory.SelectMany(o => o.PagedNews), 
                listInfoByFeed.SelectMany(o => o.PagedNews)
            }
            .SelectMany(o => o);

            foreach (var pNews in pagedNews)
            {
                var fileName = pNews.CreateFileName();
                var store = pNews.CreateSerializablePagedNews();

                Save(store, fileName);
            }
        }

        void Save(Store.News.PagedNews store, string fileName)
        {
            // TODO: write to azure storage here
        }




        #region Page Chunking functions

        ListInfoByAll CreateListInfoForAllNews()
        {
            var news = user.Feeds.AllNews().ToList();
            var pageCount = (int)Math.Ceiling((double)news.Count / (double)pageSize);

            return new ListInfoByAll
            {
                ListId = listId,
                CreatedOn = now,
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
                                ListId = listId,
                                Index = i,
                                NewsCount = o.Count,
                                News = o.Select(Convert).ToList(),
                            })
                        .ToList()
            };
        }

        ListInfoByCategory CreateListInfoForCategory(string category)
        {
            var news = user.Feeds.OfCategory(category).AllNews().ToList();
            var pageCount = (int)Math.Ceiling((double)news.Count / (double)pageSize);

            return new ListInfoByCategory
            {
                Category = category,
                ListId = listId,
                CreatedOn = now,
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
                                ListId = listId,
                                Index = i,
                                NewsCount = o.Count,
                                News = o.Select(Convert).ToList(),
                            })
                        .ToList(),
            };
        }

        ListInfoByFeed CreateListInfoForFeed(Feed feed)
        {
            var news = feed.News;
            var pageCount = (int)Math.Ceiling((double)news.Count / (double)pageSize);

            return new ListInfoByFeed
            {
                FeedId = feed.Id,
                ListId = listId,
                CreatedOn = now,
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
                                ListId = listId,
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
