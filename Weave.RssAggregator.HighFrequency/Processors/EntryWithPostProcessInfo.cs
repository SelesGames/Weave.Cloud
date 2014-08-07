using System;
using System.Linq;

namespace Weave.RssAggregator.HighFrequency
{
    public class EntryWithPostProcessInfo : Weave.Parsing.Entry
    {
        public EntryImage Image { get; private set; }
        public Images Images { get; private set; }
        public byte[] NewsItemBlob { get; set; }
        public DateTime OriginalDownloadDateTime { get; set; }

        public EntryWithPostProcessInfo()
        {
            Image = new EntryImage();
            Images = new Images();
        }

        public bool HasImage
        {
            get { return Images.Any(); }
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

    public class Image
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
    }
}