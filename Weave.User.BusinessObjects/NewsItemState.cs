using System;

namespace Weave.User.BusinessObjects
{
    public class NewsItemState
    {
        public Guid Id { get; set; }
        public bool IsFavorite { get; set; }
        public bool HasBeenViewed { get; set; }
    }
}
