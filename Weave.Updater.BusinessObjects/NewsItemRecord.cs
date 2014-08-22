using System;

namespace Weave.Updater.BusinessObjects
{
    /// <summary>
    /// Used for historical record-keeping of the last N articles for a given feed.
    /// Also used to determine when an article is a duplicate, as well as how the articles
    /// should be ordered.
    /// 
    /// Lastly, holds enough data to recreate the NewsItemIndex for canonical indices
    /// </summary>
    public class NewsItemRecord
    {
        public Guid Id { get; set; }
        public DateTime UtcPublishDateTime { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }

        // used to create the NewsItemIndex
        //public DateTime OriginalDownloadDateTime { get; set; }
        public bool HasImage { get; set; }
    }
}