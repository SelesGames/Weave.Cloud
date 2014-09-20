using System;

namespace Weave.User.BusinessObjects.Mutable
{
    public struct NewsItemIndexFeedIndexTuple : IEquatable<NewsItemIndexFeedIndexTuple>
    {
        public readonly Guid id;
        public readonly Guid feedId;
        public readonly DateTime utcPublishDateTime;
        public readonly DateTime originalDownloadDateTime;
        public readonly bool isNew;
        public readonly bool isFavorite;
        public readonly bool hasBeenViewed;
        public readonly bool hasImage;

        public NewsItemIndexFeedIndexTuple(NewsItemIndex newsItemIndex, FeedIndex feedIndex)
        {
            this.id = newsItemIndex.Id;
            this.feedId = feedIndex.Id;
            this.utcPublishDateTime = newsItemIndex.UtcPublishDateTime;
            this.originalDownloadDateTime = newsItemIndex.OriginalDownloadDateTime;
            this.isNew = feedIndex.IsNewsItemNew(newsItemIndex);
            this.isFavorite = newsItemIndex.IsFavorite;
            this.hasBeenViewed = newsItemIndex.HasBeenViewed;
            this.hasImage = newsItemIndex.HasImage;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NewsItemIndexFeedIndexTuple)obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public bool Equals(NewsItemIndexFeedIndexTuple other)
        {
            // how to do equals right, from Ben Watson
            // http://www.informit.com/articles/article.aspx?p=1567486&seqNum=2
            /* If Vertex3d were a reference type you would also need:
             * if ((object)other == null)
             *  return false;
             *
             * if (!base.Equals(other))
             *  return false;
             */

            return id == other.id;
        }
    }
}