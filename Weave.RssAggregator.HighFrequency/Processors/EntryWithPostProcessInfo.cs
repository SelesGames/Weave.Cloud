using System;

namespace Weave.RssAggregator.HighFrequency
{
    public class EntryWithPostProcessInfo : Weave.Parsing.Entry
    {
        public EntryImage Image { get; private set; }
        public byte[] NewsItemBlob { get; set; }

        public EntryWithPostProcessInfo()
        {
            Image = new EntryImage();
        }
    }

    public class EntryImage
    {
        public bool ShouldIncludeImage { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string OriginalUrl { get; set; }
        public string BaseResizedUrl { get; set; }
        public string PreferredUrl { get; set; }
        public string SupportedFormats { get; set; }
    }
}