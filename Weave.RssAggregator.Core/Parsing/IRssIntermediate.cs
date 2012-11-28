using System;
using Weave.RssAggregator.Core.DTOs.Outgoing;

namespace Weave.RssAggregator.Core.Parsing
{
    public interface IRssIntermediate
    {
        Tuple<bool, DateTime> GetTimeStamp();
        string GetPublicationDate();
        NewsItem ToNewsItem();
    }
}
