using System.Collections.Generic;
using System.Linq;

namespace Weave.UserFeedAggregator.BusinessObjects
{
    public class Image
    {
        readonly static string HIGH_REZ = "sd";
        readonly static string EXT = "jpg";

        string supportedFormats;
        List<string> supportedFormatsList;

        public int Width { get; set; }
        public int Height { get; set; }
        public string OriginalUrl { get; set; }
        public string BaseImageUrl { get; set; }

        public string SupportedFormats
        {
            get { return supportedFormats; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    supportedFormatsList = value
                        .Split(',')
                        .Where(o => !string.IsNullOrWhiteSpace(o))
                        .Select(o => o.Trim())
                        .ToList();
                }
                supportedFormats = value;
            }
        }

        public string CreateImageUrl()
        {
            if (SupportsHighRez())
            {
                return string.Format("{0}-{1}.{2}", BaseImageUrl, HIGH_REZ, EXT);
            }
            else
                return OriginalUrl;
        }

        bool SupportsHighRez()
        {
            return supportedFormatsList == null ? false : supportedFormatsList.Contains(HIGH_REZ);
        }
    }
}
