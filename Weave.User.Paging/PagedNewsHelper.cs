using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.BusinessObjects;

namespace Weave.User.Paging
{
    public class PagedNewsHelper
    {
        UserInfo user;
        int pageSize;

        public PagedNewsHelper(UserInfo user, int pageSize = 50)
        {
            this.user = user;
            this.pageSize = pageSize;
        }

        public async Task Update()
        {
            var now = DateTime.UtcNow;

            await user.RefreshAllFeeds();

            var updatedFeeds = user.Feeds.Where(o => o.LastRefreshedOn > now).ToList();

            if (updatedFeeds.Count == 0)
                return;

            var categories = updatedFeeds.Select(o => o.Category).Distinct().ToList();
            var allFeeds = user.Feeds;

            var pagedNews = new IEnumerable<PagedNewsBase>[] 
            {
                updatedFeeds.SelectMany(CreateNewsPagesForFeed),
                categories.SelectMany(CreateNewsPagesForCategory),
                CreateNewsPagesForAll(),
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

        IEnumerable<PagedNewsByCategory> CreateNewsPagesForAll()
        {
            var news = user.Feeds.AllNews().ToList();

            return Enumerable
                .Range(0, (int)Math.Ceiling((double)news.Count / (double)pageSize))
                .Select(i => news.Skip(i * pageSize).Take(pageSize).ToList())
                .Select((o, i) =>
                    new PagedNewsByCategory
                    {
                        UserId = user.Id,
                        Index = i,
                        NewsCount = o.Count,
                        News = o.Select(Convert).ToList(),
                    });
        }

        IEnumerable<PagedNewsByCategory> CreateNewsPagesForCategory(string category)
        {
            var news = user.Feeds.OfCategory(category).AllNews().ToList();

            return Enumerable
                .Range(0, (int)Math.Ceiling((double)news.Count / (double)pageSize))
                .Select(i => news.Skip(i * pageSize).Take(pageSize).ToList())
                .Select((o, i) =>
                    new PagedNewsByCategory
                    {
                        UserId = user.Id,
                        Category = category,
                        Index = i,
                        NewsCount = o.Count,
                        News = o.Select(Convert).ToList(),
                    });
        }

        IEnumerable<PagedNewsByFeed> CreateNewsPagesForFeed(Feed feed)
        {
            var news = feed.News;

            return Enumerable
                .Range(0, (int)Math.Ceiling((double)news.Count / (double)pageSize))
                .Select(i => news.Skip(i * pageSize).Take(pageSize).ToList())
                .Select((o, i) =>
                    new PagedNewsByFeed
                    {
                        UserId = user.Id,
                        FeedId = feed.Id,
                        Index = i,
                        NewsCount = o.Count,
                        News = o.Select(Convert).ToList(),
                    });
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
