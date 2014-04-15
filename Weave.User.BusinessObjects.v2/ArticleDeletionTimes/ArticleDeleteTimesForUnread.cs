using System;
using System.Collections.Generic;
using System.Linq;

namespace Weave.User.BusinessObjects.v2.ArticleDeletionTimes
{
    public class ArticleDeleteTimesForUnread : List<ArticleDeleteTime>
    {
        readonly ArticleDeleteTime defaultTime;

        public ArticleDeleteTime SelectedArticleDeleteTime { get; set; }

        public ArticleDeleteTimesForUnread()
        {
            this.Add(new ArticleDeleteTime { Display = "12 hours", Span = TimeSpan.FromHours(12) });
            this.Add(new ArticleDeleteTime { Display = "1 day", Span = TimeSpan.FromDays(1) });
            this.Add(new ArticleDeleteTime { Display = "2 days", Span = TimeSpan.FromDays(2) });
            this.Add(new ArticleDeleteTime { Display = "3 days", Span = TimeSpan.FromDays(3) });

            SelectedArticleDeleteTime = defaultTime = this[3];
        }

        public ArticleDeleteTime GetByDisplayName(string displayName)
        {
            var selected = this.FirstOrDefault(o => o.Display.Equals(displayName, StringComparison.OrdinalIgnoreCase));
            return selected ?? defaultTime;
        }
    }
}
