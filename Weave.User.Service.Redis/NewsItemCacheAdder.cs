using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.Service.Redis.DTOs;

namespace Weave.User.Service.Redis
{
    public class NewsItemCacheAdder
    {
        NewsItemCache newsCache;
        SortedNewsItemsSetCache sortedNewsCache;

        public NewsItemCacheAdder(
            NewsItemCache newsCache, 
            SortedNewsItemsSetCache sortedNewsCache)
        {
            this.newsCache = newsCache;
            this.sortedNewsCache = sortedNewsCache;
        }

        public async Task AddNews(Guid feedId, IEnumerable<NewsItem> news)
        {
            var scores = news.Select(NewsItemScoringHelper.CreateScore).ToList();

            await Task.WhenAll(
                newsCache.Set(news),
                sortedNewsCache.AddNewsItemsToSet(feedId, scores)
            );
        }
    }
}
