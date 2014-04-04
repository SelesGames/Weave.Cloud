using SelesGames.Common;
using System.Collections.Generic;
using System.Linq;
using Weave.User.BusinessObjects.Converters;
using BO = Weave.User.BusinessObjects;

namespace Weave.RssAggregator.Core.DTOs.Outgoing
{
    internal static class DtoExtensions
    {
        public static IEnumerable<BO.NewsItem> Convert(this IEnumerable<NewsItem> news)
        {
            return news
                .Select(o => o.Convert<NewsItem, BO.NewsItem>(Converters.Instance))
                .Where(o => !o.FailedToParseUtcPublishDateTime);
        }
    }
}
