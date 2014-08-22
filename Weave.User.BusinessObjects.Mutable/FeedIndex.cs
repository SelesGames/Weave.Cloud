using SelesGames.Common.Hashing;
using System;

namespace Weave.User.BusinessObjects.Mutable
{
    public class FeedIndex
    {
        public Guid Id { get; set; }
        public string Uri { get; set; }
        public string Name { get; set; }
        public string IconUri { get; set; }
        public string Category { get; set; }
        public string TeaserImageUrl { get; set; }
        public ArticleViewingType ArticleViewingType { get; set; }

        // "New" determination and bookkeeping
        public DateTime MostRecentEntrance { get; set; }
        public DateTime PreviousEntrance { get; set; }

        public NewsItemIndices NewsItemIndices { get; private set; }

        public FeedIndex()
        {
            NewsItemIndices = new NewsItemIndices(this);
        }

        public void EnsureGuidIsSet()
        {
            if (Guid.Empty.Equals(Id))
                Id = CryptoHelper.ComputeHashUsedByMobilizer(Uri);
        }

        public bool IsNewsItemNew(NewsItemIndex newsIndex)
        {
            return !newsIndex.HasBeenViewed &&
                newsIndex.OriginalDownloadDateTime > PreviousEntrance;
        }
    }
}
