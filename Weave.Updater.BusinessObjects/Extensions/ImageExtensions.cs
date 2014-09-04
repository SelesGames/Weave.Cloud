using System.Collections.Generic;
using System.Linq;

namespace Weave.Updater.BusinessObjects
{
    public static class ImageExtensions
    {
        // We use the criteria of largest image (in bytes) as being the best image available
        public static Image GetBest(this IEnumerable<Image> images)
        {
            return images.OrderByDescending(o => o.ContentLength).FirstOrDefault();
        }
    }
}
