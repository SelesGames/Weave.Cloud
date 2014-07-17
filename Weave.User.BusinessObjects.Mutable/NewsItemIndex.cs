using System;

namespace Weave.User.BusinessObjects.Mutable
{
    public class NewsItemIndex
    {
        public Guid Id { get; set; }
        public long UrlHash { get; set; }
        public long TitleHash { get; set; }
        public DateTime UtcPublishDateTime { get; set; }
        public bool IsFavorite { get; set; }
        public bool HasBeenViewed { get; set; }
        public bool HasImage { get; set; }
    }
}
