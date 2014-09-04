using System;

namespace Weave.Updater.BusinessObjects
{
    public class ExpandedEntry : Weave.Parsing.Entry
    {
        public Images Images { get; private set; }
        public byte[] NewsItemBlob { get; set; }
        public DateTime OriginalDownloadDateTime { get; set; }

        public ExpandedEntry()
        {
            Images = new Images();
        }
    }
}