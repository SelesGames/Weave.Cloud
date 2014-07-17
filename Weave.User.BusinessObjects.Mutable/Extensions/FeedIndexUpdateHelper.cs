using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.ServiceClients;

namespace Weave.User.BusinessObjects.Mutable.Extensions
{
    class FeedIndexUpdateHelper
    {
        FeedIndex feed;
        NewsServer client;

        public FeedIndexUpdateHelper(FeedIndex feed, NewsServer client)
        {
            this.feed = feed;
            this.client = client;
        }

        async Task Update()
        {
            var f = ToFeed();
            f.RefreshNews(client);
            await f.CurrentRefresh;

            var update = f.News.Select(o =>
                new
                {
                    o,
                    url = ComputeHash(o.Link),
                    title = ComputeHash(o.Title),
                });

            var newlyAdded = update.Where(o => DoesNotAlreadyContain(o.o.Id, o.url, o.title)).ToList();


            var updateSet = new UpdateSet();
            updateSet.News = newlyAdded.Select(o => o.o).ToList();
        }

        class UpdateSet
        {
            public IEnumerable<NewsItem> News { get; set; }
        }
         
        bool DoesNotAlreadyContain(Guid id, long url, long title)
        {
            return !feed.NewsItemIndices.Any(o =>
                o.Id == id ||
                o.UrlHash == url ||
                o.TitleHash == o.TitleHash
            );
        }

        long ComputeHash(string val)
        {
            var guid = CryptoHelper.ComputeHashUsedByMobilizer(val);
            var guidBytes = guid.ToByteArray();
            return BitConverter.ToInt64(guidBytes, 0);
        }

        Feed ToFeed()
        {
            return new Feed
            {
                Id = feed.Id,
                Uri = feed.Uri,
                Name = feed.Name,
                IconUri = feed.IconUri,
                Category = feed.Category,
                TeaserImageUrl = feed.TeaserImageUrl,
                ArticleViewingType = feed.ArticleViewingType,
                LastRefreshedOn = feed.LastRefreshedOn,
                Etag = feed.Etag,
                LastModified = feed.LastModified,
                MostRecentNewsItemPubDate = feed.MostRecentNewsItemPubDate,
                MostRecentEntrance = feed.MostRecentEntrance,
                PreviousEntrance = feed.PreviousEntrance,
            };
        }
    }
}
