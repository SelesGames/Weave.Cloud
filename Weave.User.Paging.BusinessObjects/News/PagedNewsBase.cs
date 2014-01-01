using System;
using System.Collections.Generic;

namespace Weave.User.Paging.News
{
    public abstract class PagedNewsBase
    {
        public Guid UserId { get; set; }
        public Guid ListId { get; set; }
        public int Index { get; set; }
        public int NewsCount { get; set; }
        public List<Store.News.NewsItem> News { get; set; }

        public abstract string CreateFileName();

        public Store.News.PagedNews CreateSerializablePagedNews()
        {
            return new Store.News.PagedNews
            {
                Index = Index,
                NewsCount = NewsCount,
                News = News,
            };
        }
    }
}
