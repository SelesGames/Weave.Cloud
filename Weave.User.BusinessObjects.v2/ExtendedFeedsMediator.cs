using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.v2
{
    public class ExtendedFeedsMediator
    {
        readonly MasterNewsItemCollection allNews;
        readonly ExtendedNewsItemsMediator extendedNewsGenerator;

        public ExtendedFeedsMediator(MasterNewsItemCollection allNews, NewsItemStateCache stateCache)
        {
            if (allNews == null) throw new ArgumentNullException("allNews");
            if (stateCache == null) throw new ArgumentNullException("stateCache");

            this.allNews = allNews;
            this.extendedNewsGenerator = new ExtendedNewsItemsMediator(stateCache);
        }

        public IEnumerable<ExtendedFeed> GetExtendedInfo(IEnumerable<Feed> feeds)
        {
            if (feeds == null) throw new ArgumentNullException("feeds");

            foreach (var feed in feeds)
            {
                IEnumerable<NewsItem> temp;
                allNews.TryGetValue(feed.Id, out temp);

                var news = temp == null ? 
                    new List<ExtendedNewsItem>(0) 
                    : 
                    extendedNewsGenerator.GetExtendedInfo(temp).ToList();

                foreach (var newsItem in news)
                    newsItem.Feed = feed;

                var extended = new ExtendedFeed(feed);
                extended.News = news;

                extended.TotalArticleCount = news.Count;
                extended.NewArticleCount = news.Count(o => o.IsCountedAsNew());
                extended.UnreadArticleCount = news.Count(o => !o.HasBeenViewed);

                yield return extended;
            }
        }
    }
}
