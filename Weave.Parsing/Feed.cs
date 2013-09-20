using Common.Net.Http.Compression;
using Common.TimeFormatting;
using SelesGames.Common.Hashing;
using SelesGames.WebApi.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Weave.Parsing.Intermediates;

namespace Weave.Parsing
{
    public class Feed
    {
        public Guid FeedId { get; set; }
        public string FeedUri { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public string OldestNewsItemPubDate { get; private set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public TimeSpan UpdateTimeOut { get; set; }

        public RequestStatus Status { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string DomainUrl { get; private set; }
        public List<Entry> News { get; private set; }

        public enum RequestStatus
        {
            OK,
            Unmodified
        }


        public Feed()
        {
            this.News = new List<Entry>();
            this.UpdateTimeOut = TimeSpan.FromMinutes(1);

#if DEBUG
            this.UpdateTimeOut = TimeSpan.FromHours(1);
#endif
        }

        public async Task<RequestStatus> Update()
        {
            EnsureFeedIdIsSet();

            var handler = new HttpClientCompressionHandler();

            var request = new HttpClient(handler);
            request.Timeout = UpdateTimeOut;
            request.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", new[] { "gzip", "deflate" });


            #region CONDITIONAL GET

            //if (!string.IsNullOrEmpty(Etag))
            //{
            //    request.DefaultRequestHeaders.IfNoneMatch.TryParseAdd(Etag);
            //}

            //if (!string.IsNullOrEmpty(LastModified))
            //{
            //    DateTime lastModified;
            //    if (DateTime.TryParse(LastModified, out lastModified))
            //        request.DefaultRequestHeaders.IfModifiedSince = lastModified;
            //}

            if (!string.IsNullOrEmpty(Etag))
            {
                request.DefaultRequestHeaders.TryAddWithoutValidation("If-None-Match", Etag);
            }

            if (!string.IsNullOrEmpty(LastModified))
            {
                request.DefaultRequestHeaders.TryAddWithoutValidation("If-Modified-Since", LastModified);
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

        async Task HandleNewData(HttpResponseMessage response)
        {
            #region CONDITIONAL GET

            Etag = response.Headers.GetValueForHeader("ETag");
            LastModified = response.Content.Headers.GetValueForHeader("Last-Modified");

            #endregion

            if (response.Content == null)
                return;

            var byteContent = await response.Content.ReadAsByteArrayAsync();
            using (var ms = new MemoryStream(byteContent))
            {
                ms.Position = 0;
                ParseFeedMetaData(ms);
                ms.Position = 0;
                ParseNewsFromLastRefreshTime(ms);
                ms.Close();
            }
        }

        void ParseFeedMetaData(MemoryStream ms)
        {
            XElement root = null;

            var doc = XDocument.Load(ms, LoadOptions.None);

            root = doc.Root;
            var channel = doc.Descendants("channel").FirstOrDefault();

            if (channel != null)
                root = channel;

            Name = root.Element("title").ValueOrDefault(null);
            Description = root.Element("description").ValueOrDefault(null);
            DomainUrl = root.Element("link").ValueOrDefault(null);
        }

        void ParseNewsFromLastRefreshTime(MemoryStream ms)
        {
            var intermediates = ms.ToRssIntermediates().ToList();
            var orderedNews = intermediates.OrderByDescending(o => o.PublicationDate);
            var previousMostRecentNewsItemPubDateString = this.MostRecentNewsItemPubDate;


            var mostRecentItem = orderedNews.FirstOrDefault();
            if (mostRecentItem != null)
                this.MostRecentNewsItemPubDate = mostRecentItem.PublicationDateString;

            var oldestItem = orderedNews.LastOrDefault();
            if (oldestItem != null)
                this.OldestNewsItemPubDate = oldestItem.PublicationDateString;


            IEnumerable<IEntryIntermediate> filteredNews = orderedNews;

            // If previous MostRecentNewsItemPubDate has been set, then take only the news that is more recent than that
            if (!string.IsNullOrWhiteSpace(previousMostRecentNewsItemPubDateString))
            {
                var tryGetPreviousMostRecentDate = previousMostRecentNewsItemPubDateString.TryGetUtcDate();
                if (tryGetPreviousMostRecentDate.Item1)
                {
                    var previousMostRecentNewsItemPubDate = tryGetPreviousMostRecentDate.Item2;
                    filteredNews = orderedNews.TakeWhile(o => o.PublicationDate > previousMostRecentNewsItemPubDate);
                }
            }

            this.News = filteredNews.Select(TryCreateEntryFromIntermediate).OfType<Entry>().ToList();

            var now = DateTime.UtcNow;

            foreach (var newsItem in News)
            {
                newsItem.FeedId = FeedId;
                newsItem.Id = CryptoHelper.ComputeHashUsedByMobilizer(newsItem.Link + FeedUri);

                // cap the publishdatetime as no later than the current time
                if (newsItem.UtcPublishDateTime > now)
                    newsItem.UtcPublishDateTime = now;
            }

            this.Status = RequestStatus.OK;
        }

        Entry TryCreateEntryFromIntermediate(IEntryIntermediate intermediate)
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

        public override string ToString()
        {
            return FeedUri;
        }
    }
}
