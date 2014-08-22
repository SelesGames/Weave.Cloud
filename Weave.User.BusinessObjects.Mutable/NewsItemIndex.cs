﻿using System;

namespace Weave.User.BusinessObjects.Mutable
{
    public class NewsItemIndex
    {
        public Guid Id { get; set; }
        public DateTime UtcPublishDateTime { get; set; }
        public DateTime OriginalDownloadDateTime { get; set; }
        public bool IsFavorite { get; set; }
        public bool HasBeenViewed { get; set; }
        public bool HasImage { get; set; }

        // reference to the parent FeedIndex. Should not be serialized
        public FeedIndex FeedIndex { get; set; }
    }
}
