using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Weave.RssAggregator.Parsing
{
    public class FeedRequester
    {
        public Guid FeedId { get; set; }
        public string FeedUri { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public string OldestNewsItemPubDate { get; private set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public TimeSpan TimeOut { get; set; }
        public List<Entry> News { get; private set; }
        public RequestStatus Status { get; private set; }

        public enum RequestStatus
        {
            OK,
            Unmodified
        }


        public FeedRequester()
        {
            this.News = new List<Entry>();
            this.TimeOut = TimeSpan.FromMinutes(1);
        }

        public async Task<RequestStatus> UpdateFeed()
        {
            EnsureFeedIdIsSet();

            var request = new HttpClient();
            request.Timeout = TimeOut;

            #region CONDITIONAL GET

            if (!string.IsNullOrEmpty(Etag))
            {
                request.DefaultRequestHeaders.IfNoneMatch.TryParseAdd(Etag);
            }

            if (!string.IsNullOrEmpty(LastModified))
            {
                DateTime lastModified;
                if (DateTime.TryParse(LastModified, out lastModified))
                    request.DefaultRequestHeaders.IfModifiedSince = lastModified;
            }

            #endregion

            var response = await request.GetAsync(FeedUri);

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                HandleNonModifiedResponse();
                return Status;
            }

            else if (response.StatusCode == HttpStatusCode.OK)
            {
                await HandleNewData(response);
                return Status;
            }

            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
        }

        void EnsureFeedIdIsSet()
        {
            if (FeedId.Equals(Guid.Empty))
                FeedId = CryptoHelper.ComputeHashUsedByMobilizer(FeedUri);
        }

        void HandleNonModifiedResponse()
        {
            this.Status = RequestStatus.Unmodified;
        }

        async Task HandleNewData(HttpResponseMessage o)
        {
            #region CONDITIONAL GET

            Etag = GetEtag(o);

            var lm = o.Content.Headers.LastModified;
            if (lm != null && lm.HasValue)
            {
                var lastModified = lm.Value.ToString();
                if (!string.IsNullOrEmpty(lastModified))
                    LastModified = lastModified;
            }

            #endregion


            using (var stream = await o.Content.ReadAsStreamAsync())
            {
                ParseNewsFromLastRefreshTime(stream);
                stream.Close();
            }
        }

        string GetEtag(HttpResponseMessage response)
        {
            var headers = response.Headers;

            if (headers == null) return null;

            string eTag = null;

            if (headers.ETag != null)
                eTag = headers.ETag.Tag;
            else
            {
                IEnumerable<string> headerValues;
                if (headers.TryGetValues("etag", out headerValues))
                    eTag = headerValues.FirstOrDefault();
            }

            return eTag;
        }

        void ParseNewsFromLastRefreshTime(Stream stream)
        {
            var intermediates = stream.ToRssIntermediates().ToList();
            var orderedNews = intermediates.OrderByDescending(o => o.PublicationDate);
            var previousMostRecentNewsItemPubDateString = this.MostRecentNewsItemPubDate;


            var mostRecentItem = orderedNews.FirstOrDefault();
            if (mostRecentItem != null)
                this.MostRecentNewsItemPubDate = mostRecentItem.PublicationDateString;

            var oldestItem = orderedNews.LastOrDefault();
            if (oldestItem != null)
                this.OldestNewsItemPubDate = oldestItem.PublicationDateString;


            IEnumerable<IEntryIntermediate> filteredNews = orderedNews;

            var tryGetPreviousMostRecentDate = previousMostRecentNewsItemPubDateString.TryGetUtcDate();
            if (tryGetPreviousMostRecentDate.Item1)
            {
                var previousMostRecentNewsItemPubDate = tryGetPreviousMostRecentDate.Item2;
                filteredNews = orderedNews.TakeWhile(o => o.PublicationDate > previousMostRecentNewsItemPubDate);
            }

            var news = filteredNews.Select(TryCreateNewsItem).OfType<Entry>().ToList();
            this.News = news;

            foreach (var newsItem in News)
                newsItem.FeedId = FeedId;

            this.Status = RequestStatus.OK;
        }

        Entry TryCreateNewsItem(IEntryIntermediate intermediate)
        {
            try
            {
                return intermediate.CreateEntry();
            }
            catch (Exception e)
            {
                DebugEx.WriteLine(e);
                return null;
            }
        }
    }
}
