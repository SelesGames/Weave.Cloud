
namespace Weave.User.BusinessObjects.Mutable
{
    public class NewsItemIndexFeedIndexTuple
    {
        public NewsItemIndex NewsItemIndex { get; private set; }
        public FeedIndex FeedIndex { get; private set; }
        public bool IsNew { get; private set; }

        public NewsItemIndexFeedIndexTuple(NewsItemIndex newsItemIndex, FeedIndex feedIndex)
        {
            NewsItemIndex = newsItemIndex;
            FeedIndex = feedIndex;
            IsNew = feedIndex.IsNewsItemNew(newsItemIndex);
        }
    }
}
