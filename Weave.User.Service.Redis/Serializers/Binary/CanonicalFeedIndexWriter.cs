using System;
using System.IO;
using System.Text;
using Weave.User.BusinessObjects.Mutable;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class CanonicalFeedIndexWriter : IDisposable
    {
        readonly MemoryStream ms;
        readonly BinaryWriter bw;
        readonly FeedIndex feed;

        internal CanonicalFeedIndexWriter(FeedIndex feed)
        {
            this.feed = feed;
            ms = new MemoryStream();
            bw = new BinaryWriter(ms, Encoding.UTF8);
        }

        internal byte[] GetBytes()
        {
            return ms.ToArray();
        }

        public void Write()
        {

            // CERTAIN VALUES NO LONGER HOLD TRUE FOR CANONICAL FEEDS
            // since this feed data is not specific to any particular user, the
            // following values will be omitted.  NOTE - the ID is no longer 
            // valid either, since a feed's ID is really specific to a user
            //bw.Write(feed.Id);
            //bw.Write(feed.Name ?? "");
            //bw.Write(feed.Category ?? "");
            //bw.Write(feed.MostRecentEntrance.ToBinary());
            //bw.Write(feed.PreviousEntrance.ToBinary());
            //bw.Write((int)feed.ArticleViewingType);

            // write string values
            bw.Write(feed.Uri ?? "");
            bw.Write(feed.IconUri ?? "");
            bw.Write(feed.TeaserImageUrl ?? "");
            bw.Write(feed.Etag ?? "");
            bw.Write(feed.LastModified ?? "");
            bw.Write(feed.MostRecentNewsItemPubDate ?? "");

            // write DateTime values
            bw.Write(feed.LastRefreshedOn);

            if (feed.NewsItemIndices != null)
            {
                bw.Write(feed.NewsItemIndices.Count);

                foreach (var newsItem in feed.NewsItemIndices)
                    WriteNewsItemIndex(newsItem);
            }
            else
            {
                bw.Write(0);
            }
        }

        void WriteNewsItemIndex(NewsItemIndex newsItem)
        {
            bw.Write(newsItem.Id);

            // CERTAIN VALUES NO LONGER HOLD TRUE FOR CANONICAL NEWS ITEM INDICES
            // since this news item data is not specific to any particular user, the
            // following values will be omitted:
            //bw.Write(newsItem.IsFavorite);
            //bw.Write(newsItem.HasBeenViewed);

            bw.Write(newsItem.UtcPublishDateTime);
            bw.Write(newsItem.OriginalDownloadDateTime);
            bw.Write(newsItem.HasImage);
        }

        public void Dispose()
        {
            bw.Close();
            ms.Close();
            bw.Dispose();
            ms.Dispose();
        }
    }
}