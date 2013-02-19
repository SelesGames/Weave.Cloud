using System;

namespace Weave.RssAggregator.Client.Parsing.Intermediates
{
    internal interface IEntryIntermediate
    {
        DateTime PublicationDate { get; set; }
        string PublicationDateString { get; set; }
        Entry CreateEntry();
    }
}
