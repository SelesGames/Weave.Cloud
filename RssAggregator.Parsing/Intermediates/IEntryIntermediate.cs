using System;

namespace Weave.RssAggregator.Parsing
{
    public interface IEntryIntermediate
    {
        DateTime PublicationDate { get; set; }
        string PublicationDateString { get; set; }
        Entry CreateEntry();
    }
}
