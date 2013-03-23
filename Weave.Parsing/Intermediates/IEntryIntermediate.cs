using System;

namespace Weave.Parsing.Intermediates
{
    public interface IEntryIntermediate
    {
        DateTime PublicationDate { get; set; }
        string PublicationDateString { get; set; }
        Entry CreateEntry();
    }
}
