using SelesGames.Common.Hashing;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Weave.User.BusinessObjects.v2
{
    public class Feed
    {
        public Guid Id { get; set; }
        public string Uri { get; set; }
        public string Name { get; set; }
        public string IconUri { get; set; }
        public string Category { get; set; }
        public ArticleViewingType ArticleViewingType { get; set; }
        public string TeaserImageUrl { get; set; }

        // feed updating
        public string Etag { get; set; }
        public string LastModified { get; set; }

        public string MostRecentNewsItemPubDate { get; set; }
        public DateTime LastRefreshedOn { get; set; }

        // "New" determination and bookkeeping
        public DateTime MostRecentEntrance { get; set; }
        public DateTime PreviousEntrance { get; set; }

        public List<Guid> NewsItemIds { get; set; }

        public void EnsureGuidIsSet()
        {
            if (Guid.Empty.Equals(Id))
                Id = CryptoHelper.ComputeHashUsedByMobilizer(Uri);
        }
    }
}
