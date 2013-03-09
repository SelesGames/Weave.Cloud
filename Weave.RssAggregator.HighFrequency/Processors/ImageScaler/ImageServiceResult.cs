using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weave.RssAggregator.HighFrequency
{
    public class ImageServiceResult
    {
        string supportedFormats;
        List<string> supportedFormatsList;

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public string BaseImageUrl { get; set; }
        public string FileExtension { get; set; }
        public List<string> SavedFileNames { get; set; }

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

        public List<string> SupportedFormatsList
        {
            get
            {
                return supportedFormatsList;
            }
        }
    }
}
