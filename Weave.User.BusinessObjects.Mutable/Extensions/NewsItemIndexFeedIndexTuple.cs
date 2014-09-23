using System;

namespace Weave.User.BusinessObjects.Mutable
{
    //public struct NewsItemIndexFeedIndexTuple : IEquatable<NewsItemIndexFeedIndexTuple>
    //{
    //    public readonly Guid Id;
    //    public readonly Guid FeedId;
    //    public readonly DateTime UtcPublishDateTime;
    //    public readonly DateTime OriginalDownloadDateTime;
    //    public readonly bool CanKeep;
    //    public readonly bool IsNew;
    //    public readonly bool IsFavorite;
    //    public readonly bool HasBeenViewed;
    //    public readonly bool HasImage;

    //    public NewsItemIndexFeedIndexTuple(
    //        NewsItemIndex newsItemIndex, 
    //        FeedIndex feedIndex, 
    //        DateTime markedReadCutoffDate, 
    //        DateTime unreadCutoffDate)
    //    {
    //        this.Id = newsItemIndex.Id;
    //        this.FeedId = feedIndex.Id;
    //        this.UtcPublishDateTime = newsItemIndex.UtcPublishDateTime;
    //        this.OriginalDownloadDateTime = newsItemIndex.OriginalDownloadDateTime;
    //        this.IsNew = feedIndex.IsNewsItemNew(newsItemIndex);
    //        this.IsFavorite = newsItemIndex.IsFavorite;
    //        this.HasBeenViewed = newsItemIndex.HasBeenViewed;
    //        this.HasImage = newsItemIndex.HasImage;

    //        // set canKeep
    //        if (IsNew)
    //            CanKeep = true;

    //        else if (IsFavorite)
    //            CanKeep = true;

    //        else if (HasBeenViewed && OriginalDownloadDateTime > markedReadCutoffDate)
    //            CanKeep = true;

    //        else if (!HasBeenViewed && OriginalDownloadDateTime > unreadCutoffDate)
    //            CanKeep = true;

    //        else
    //            CanKeep = false;
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        if (obj == null) return false;
    //        if (obj.GetType() != this.GetType()) return false;
    //        return Equals((NewsItemIndexFeedIndexTuple)obj);
    //    }

    //    public override int GetHashCode()
    //    {
    //        return Id.GetHashCode();
    //    }

    //    public bool Equals(NewsItemIndexFeedIndexTuple other)
    //    {
    //        // how to do equals right, from Ben Watson
    //        // http://www.informit.com/articles/article.aspx?p=1567486&seqNum=2
    //        /* If Vertex3d were a reference type you would also need:
    //         * if ((object)other == null)
    //         *  return false;
    //         *
    //         * if (!base.Equals(other))
    //         *  return false;
    //         */

    //        return Id == other.Id;
    //    }
    //}


    // same struct, but minimize amount of data being copied
    public struct NewsItemIndexFeedIndexTuple : IEquatable<NewsItemIndexFeedIndexTuple>
    {
        readonly NewsItemIndex newsItem;
        readonly FeedIndex feed;

        public readonly bool CanKeep;
        public readonly bool IsNew;


        public Guid Id { get { return newsItem.Id; } }
        public Guid FeedId { get { return feed.Id; } }
        public DateTime UtcPublishDateTime { get { return newsItem.UtcPublishDateTime; } }
        public DateTime OriginalDownloadDateTime { get { return newsItem.OriginalDownloadDateTime; } }
        public bool IsFavorite { get { return newsItem.IsFavorite; } }
        public bool HasBeenViewed { get { return newsItem.HasBeenViewed; } }
        public bool HasImage { get { return newsItem.HasImage; } }

        public NewsItemIndexFeedIndexTuple(
            NewsItemIndex newsItem,
            FeedIndex feed,
            DateTime markedReadCutoffDate,
            DateTime unreadCutoffDate)
        {
            this.newsItem = newsItem;
            this.feed = feed;
            this.IsNew = feed.IsNewsItemNew(newsItem);

            // set canKeep
            if (IsNew)
                CanKeep = true;

            else if (newsItem.IsFavorite)
                CanKeep = true;

            else if (newsItem.HasBeenViewed && newsItem.OriginalDownloadDateTime > markedReadCutoffDate)
                CanKeep = true;

            else if (!newsItem.HasBeenViewed && newsItem.OriginalDownloadDateTime > unreadCutoffDate)
                CanKeep = true;

            else
                CanKeep = false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NewsItemIndexFeedIndexTuple)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
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

            return Id == other.Id;
        }
    }
}