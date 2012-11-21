﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Weave.RssAggregator.Core.DTOs.Outgoing;
using Weave.RssAggregator.Core.Parsing;

namespace Weave.RssAggregator.Core
{
    public class FeedRequester
    {
        public TimeSpan TimeOut { get; set; }
        public string FeedUri { get; set; }
        public string MostRecentNewsItemPubDate { get; set; }
        public string OldestNewsItemPubDate { get; private set; }
        public string Etag { get; set; }
        public string LastModified { get; set; }
        public List<NewsItem> News { get; private set; }
        public RequestStatus Status { get; private set; }

        public enum RequestStatus
        {
            OK,
            Unmodified
        }


        public FeedRequester()
        {
            this.News = new List<NewsItem>();
            this.TimeOut = TimeSpan.FromMinutes(1);
        }


        public async Task<RequestStatus> UpdateFeed()
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(FeedUri);



            #region CONDITIONAL GET

            if (!string.IsNullOrEmpty(Etag))
            {
                request.Headers[HttpRequestHeader.IfNoneMatch] = Etag;
            }

            if (!string.IsNullOrEmpty(LastModified))
            {
                DateTime lastModified;
                if (DateTime.TryParse(LastModified, out lastModified))
                    request.IfModifiedSince = lastModified;
            }

            #endregion



            var temp = await request.GetResponseAsync().ConfigureAwait(false);
            var response = (HttpWebResponse)temp;

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                HandleNonModifiedResponse();
                return Status;
            }

            else if (response.StatusCode == HttpStatusCode.OK)
            {
                HandleNewData(response);
                return Status;
            }

            else
            {
                throw new Exception(response.StatusCode.ToString());
            }
        }

        void HandleNonModifiedResponse()
        {
            //DebugEx.WriteLine(string.Format("SERVER SIDE ### Using cached data for {0}", this.FeedUri));
            this.Status = RequestStatus.Unmodified;
        }

        void HandleNewData(HttpWebResponse o)
        {
            //DebugEx.WriteLine(string.Format("SERVER SIDE Refreshing {0} with new data", this.FeedUri));


            #region CONDITIONAL GET

            var eTag = o.Headers[HttpResponseHeader.ETag];
            if (!string.IsNullOrEmpty(eTag))
                this.Etag = eTag;

            var lastModified = o.Headers[HttpResponseHeader.LastModified];
            if (!string.IsNullOrEmpty(lastModified))
                this.LastModified = lastModified;

            #endregion


            using (var stream = o.GetResponseStream())
            {
                ParseNewsFromLastRefreshTime(stream);
                stream.Close();
            }
        }

        void ParseNewsFromLastRefreshTime(Stream stream)
        {
            #region new approach using switching parsing (weave + Syndicationfeed)

            var elementsWithDate = stream
                .ToRssIntermediates()
                .ToRssIntermediatesWithDate()
                //.ToList()
                .OrderByDescending(o => o.Item1);


            var previousMostRecentNewsItemPubDateString = this.MostRecentNewsItemPubDate;


            var mostRecentItem = elementsWithDate.FirstOrDefault();
            if (mostRecentItem != null)
                this.MostRecentNewsItemPubDate = mostRecentItem.Item2.GetPublicationDate();

            var oldestItem = elementsWithDate.LastOrDefault();
            if (oldestItem != null)
                this.OldestNewsItemPubDate = oldestItem.Item2.GetPublicationDate();


            IEnumerable<Tuple<DateTime, IRssIntermediate>> filteredNews = elementsWithDate;

            var tryGetPreviousMostRecentDate = RssHelperFunctions.TryGetUtcDate(previousMostRecentNewsItemPubDateString);
            if (tryGetPreviousMostRecentDate.Item1)
            {
                var previousMostRecentNewsItemPubDate = tryGetPreviousMostRecentDate.Item2;
                filteredNews = elementsWithDate.TakeWhile(o => o.Item1 > previousMostRecentNewsItemPubDate);
            }

            var news = filteredNews
                .Select(o => o.Item2.ToNewsItem())
                .OfType<NewsItem>()
                .ToList();

            this.News = news;

            this.Status = RequestStatus.OK;

            #endregion




            #region old approach using stricly Weave parsing

            //var elementsWithDate = stream
            //    .ToXElements()
            //    .ToXElementsWithDate()
            //    .OrderByDescending(o => o.Item1);


            //var previousMostRecentNewsItemPubDateString = this.MostRecentNewsItemPubDate;


            //var mostRecentItem = elementsWithDate.FirstOrDefault();
            //if (mostRecentItem != null)
            //    this.MostRecentNewsItemPubDate = mostRecentItem.Item2.Element("pubDate").ValueOrDefault();

            //var oldestItem = elementsWithDate.LastOrDefault();
            //if (oldestItem != null)
            //    this.OldestNewsItemPubDate = oldestItem.Item2.Element("pubDate").ValueOrDefault();


            //IEnumerable<Tuple<DateTime, XElement>> filteredNews = elementsWithDate;

            //var tryGetPreviousMostRecentDate = RssServiceLayer.TryGetUtcDate(previousMostRecentNewsItemPubDateString);
            //if (tryGetPreviousMostRecentDate.Item1)
            //{
            //    var previousMostRecentNewsItemPubDate = tryGetPreviousMostRecentDate.Item2;
            //    filteredNews = elementsWithDate.TakeWhile(o => o.Item1 > previousMostRecentNewsItemPubDate);
            //}

            //var news = filteredNews
            //    .Select(o => o.Item2.ToNewsItem())
            //    .OfType<NewsItem>()
            //    .ToList();

            //this.News = news;

            //this.Status = RequestStatus.OK;

            #endregion
        }
    }
}