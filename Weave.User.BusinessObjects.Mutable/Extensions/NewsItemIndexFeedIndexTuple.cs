using System;

namespace Weave.User.BusinessObjects.Mutable
{
    public struct NewsItemIndexFeedIndexTuple : IEquatable<NewsItemIndexFeedIndexTuple>
    {
        readonly Guid id;
        readonly Guid feedId;
        readonly NewsItemIndex newsItemIndex;
        //readonly FeedIndex feedIndex;
        readonly bool isNew;
        readonly DateTime publishDateTime;

        public NewsItemIndex NewsItemIndex { get { return newsItemIndex; } }
        //public FeedIndex FeedIndex { get { return feedIndex; } }
        public DateTime UtcPublishDateTime { get { return publishDateTime; } }
        public bool IsNew { get { return isNew; } }
        public Guid FeedId { get { return feedId; } }

        //public NewsItemIndex NewsItemIndex { get; private set; }
        //public FeedIndex FeedIndex { get; private set; }
        //public bool IsNew { get; private set; }

        public NewsItemIndexFeedIndexTuple(NewsItemIndex newsItemIndex, FeedIndex feedIndex)
        {
            //NewsItemIndex = newsItemIndex;
            //FeedIndex = feedIndex;
            //IsNew = feedIndex.IsNewsItemNew(newsItemIndex);
            this.id = newsItemIndex.Id;
            this.newsItemIndex = newsItemIndex;
            //this.feedIndex = feedIndex;
            this.feedId = feedIndex.Id;
            this.publishDateTime = newsItemIndex.UtcPublishDateTime;
            this.isNew = feedIndex.IsNewsItemNew(newsItemIndex);
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