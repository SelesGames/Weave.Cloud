﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.User.BusinessObjects.ServiceClients;

namespace Weave.User.BusinessObjects
{
    public class FeedsSubset : IEnumerable<Feed>
    {
        List<Feed> feeds;

        public FeedsSubset(IEnumerable<Feed> feeds)
        {
            this.feeds = new List<Feed>(feeds);
        }

        public async Task Refresh()
        {
            if (EnumerableEx.IsNullOrEmpty(feeds))
                return;

            var client = new NewsServer();
            foreach (var feed in feeds)
                feed.RefreshNews(client);

            client.SendRequests();
            await Task.WhenAll(feeds.Select(o => o.CurrentRefresh));
        }

        public void MarkEntry()
        {
            if (EnumerableEx.IsNullOrEmpty(feeds))
                return;

            var now = DateTime.UtcNow;
            foreach (var feed in feeds)
            {
                feed.PreviousEntrance = feed.MostRecentEntrance;
                feed.MostRecentEntrance = now;
            }
        }

        public void ExtendEntry()
        {
            if (EnumerableEx.IsNullOrEmpty(feeds))
                return;

            var now = DateTime.UtcNow;
            foreach (var feed in feeds)
            {
                feed.MostRecentEntrance = now;
            }
        }




        #region IEnumerable interface implementation

        public IEnumerator<Feed> GetEnumerator()
        {
            return feeds.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return feeds.GetEnumerator();
        }

        #endregion
    }
}
