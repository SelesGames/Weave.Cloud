using System.Collections.Generic;
using System.Linq;
using Weave.Parsing;

namespace Weave.Updater.BusinessObjects
{
    public static class EntryExtensions
    {
        public static string GetBestImageUrl(this IEnumerable<Entry> entries)
        {
            var mostRecentAndBestImage = entries
                .OrderByDescending(o => o.UtcPublishDateTime)
                .Select(o => o.ImageUrls.FirstOrDefault())
                .OfType<string>()
                .FirstOrDefault();

            return mostRecentAndBestImage;
        }

        public static Image GetBestImage(this IEnumerable<ExpandedEntry> entries)
        {
            var mostRecentAndBestImage = entries
                .OrderByDescending(o => o.UtcPublishDateTime)
                .Select(o => o.Images.GetBest())
                .OfType<Image>()
                .FirstOrDefault();

            return mostRecentAndBestImage;
        }
    }
}
