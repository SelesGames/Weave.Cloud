using System;
using System.Linq;

namespace Weave.RssAggregator.HighFrequency
{
    public class EntryWithPostProcessInfo : Weave.Parsing.Entry
    {
        public Images Images { get; private set; }
        public byte[] NewsItemBlob { get; set; }
        public DateTime OriginalDownloadDateTime { get; set; }

        public EntryWithPostProcessInfo()
        {
            Images = new Images();
        }
    }
}