using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.User.BusinessObjects.Comparers;
//using Weave.User.BusinessObjects.ServiceClients;

namespace Weave.User.BusinessObjects
{
    public class Feed
    {
        #region Private member variables

        static readonly int TRIM = 100;
        static readonly NewsItemTitleComparer newsItemTitleComparer = new NewsItemTitleComparer(168d);

        bool isUpdating = false;
        List<NewsItem> news;

        #endregion




        #region Public Properties

        public Guid Id { get; set; }
        public UserInfo User { get; set; }
        public string Uri { get; set; }
        public string Name { get; set; }
        public string IconUri { get; set; }
        public string Category { get; set; }
        public string TeaserImageUrl { get; set; }
        public ArticleViewingType ArticleViewingType { get; set; }

        // record-keeping for feed updates
        public DateTime LastRefreshedOn { get; set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }

        // "New" determination and bookkeeping
        public DateTime MostRecentEntrance { get; set; }
        public DateTime PreviousEntrance { get; set; }
        
        public IReadOnlyList<NewsItem> News
        {
            get { return news; }
            set
            {
                news = value == null ? null : value.ToList();
                //UpdateTeaserImage();
            }
        }

        // Read-only properties
        //public Task CurrentRefresh { get; private set; }

        #endregion




        public void EnsureGuidIsSet()
        {
            if (Guid.Empty.Equals(Id))
                Id = CryptoHelper.ComputeHashUsedByMobilizer(Uri);
        }   
    }
}




#region all feed refreshing is now deprecated

 //#region Refresh News

 //       public void RefreshNews(NewsServer client, Action<Exception> onError = null)
 //       {
 //           if (isUpdating)
 //               return;

 //           CurrentRefresh = refreshNews(client, onError);
 //       }

 //       async Task refreshNews(NewsServer client, Action<Exception> onError = null)
 //       {
 //           if (isUpdating)
 //               return;

 //           isUpdating = true;

 //           if (client == null)
 //               throw new Exception("no NewsServer was registered via the Service Resolver");

 //           var updatedRequest = new Request
 //           {
 //               Id = Id.ToString(),
 //               Etag = Etag,
 //               Url = Uri,
 //               LastModified = LastModified,
 //               MostRecentNewsItemPubDate = MostRecentNewsItemPubDate,
 //           };

 //           try
 //           {
 //               var update = await client.GetFeedResultAsync(updatedRequest).ConfigureAwait(false);
 //               HandleUpdate(update);
 //           }
 //           catch (Exception exception)
 //           {
 //               DebugEx.WriteLine(exception.Message);
 //               if (onError != null)
 //                   onError(exception);
 //           }

 //           isUpdating = false;
 //       }

 //       #endregion




 //       #region Helper functions for handling an update

 //       void HandleUpdate(FeedResult update)
 //       {
 //           if (update == null ||
 //               update.Status != FeedResultStatus.OK || 
 //               EnumerableEx.IsNullOrEmpty(update.News))
 //               return;

 //           var now = DateTime.UtcNow;
 //           news = news ?? new List<NewsItem>();
 //           var previousNews = news;

 //           var updatedNews = update.News.Convert().ToList();

 //           foreach (var newsItem in updatedNews)
 //           {
 //               newsItem.OriginalDownloadDateTime = now;
 //               newsItem.Feed = this;
 //           }

 //           var mergedNews = news
 //               .Union(updatedNews, newsItemTitleComparer)
 //               .OrderByDescending(o => o.IsNew())
 //               .ThenByDescending(o => o.UtcPublishDateTime)
 //               .Take(TRIM)
 //               .ToList();

 //           news = mergedNews;

 //           if (news.IsSetEqualTo(previousNews, new NewsItemIdComparer()))
 //               return;

 //           LastRefreshedOn = now;
 //           IconUri = update.IconUri;
 //           Etag = update.Etag;
 //           LastModified = update.LastModified;
 //           MostRecentNewsItemPubDate = update.MostRecentNewsItemPubDate;
 //           UpdateTeaserImage();
 //       }

 //       void UpdateTeaserImage()
 //       {
 //           if (EnumerableEx.IsNullOrEmpty(news))
 //               return;

 //           TeaserImageUrl = news
 //               .OrderByDescending(o => o.UtcPublishDateTime)
 //               .Where(o => o.HasImage)
 //               .Select(o => o.GetBestImageUrl())
 //               .FirstOrDefault();
 //       }

 //       #endregion

#endregion