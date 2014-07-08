using System;

namespace Weave.User.BusinessObjects.v2.User
{
    public class NewsItemIndex
    {
        public Guid Id { get; set; }
        public DateTime UtcPublishDateTime { get; private set; }
        public bool IsFavorite { get; set; }
        public bool HasBeenViewed { get; set; }
    }
}
