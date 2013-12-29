using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.ArticleDeletionTimes
{
    internal class ArticleDeleteTimesForMarkedRead : List<ArticleDeleteTime>
    {
        readonly ArticleDeleteTime defaultTime;

        public ArticleDeleteTime SelectedArticleDeleteTime { get; set; }

        public ArticleDeleteTimesForMarkedRead()
        {
            this.Add(new ArticleDeleteTime { Display = "30 minutes", Span = TimeSpan.FromMinutes(30) });
            this.Add(new ArticleDeleteTime { Display = "2 hours", Span = TimeSpan.FromHours(3) });
            this.Add(new ArticleDeleteTime { Display = "6 hours", Span = TimeSpan.FromHours(6) });
            this.Add(new ArticleDeleteTime { Display = "12 hours", Span = TimeSpan.FromHours(12) });

            SelectedArticleDeleteTime = defaultTime = this[2];
        }

        public ArticleDeleteTime GetByDisplayName(string displayName)
        {
            var selected = this.FirstOrDefault(o => o.Display.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            return selected ?? defaultTime;
        }
    }
}
