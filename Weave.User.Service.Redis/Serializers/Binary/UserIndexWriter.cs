using System;
using System.IO;
using System.Text;
using Weave.User.BusinessObjects.Mutable;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class UserIndexWriter : IDisposable
    {
        readonly MemoryStream ms;
        readonly BinaryWriter bw;
        readonly UserIndex user;

        internal UserIndexWriter(UserIndex user)
        {
            this.user = user;
            ms = new MemoryStream();
            bw = new BinaryWriter(ms, Encoding.UTF8);
        }

        internal byte[] GetBytes()
        {
            return ms.ToArray();
        }

        internal void Write()
        {
            bw.Write(user.Id);
            bw.Write(user.PreviousLoginTime);
            bw.Write(user.CurrentLoginTime);

            // write string values
            bw.Write(user.ArticleDeletionTimeForMarkedRead ?? "");
            bw.Write(user.ArticleDeletionTimeForUnread ?? "");

            if (user.FeedIndices != null)
            {
                bw.Write(user.FeedIndices.Count);

                foreach (var feed in user.FeedIndices)
                    WriteFeedIndex(feed);
            }
            else
            {
                bw.Write(0);
            }
        }

        void WriteFeedIndex(FeedIndex feed)
        {
            bw.Write(feed.Id);

            // write string values
            bw.Write(feed.Uri ?? "");
            bw.Write(feed.Name ?? "");
            bw.Write(feed.IconUri ?? "");
            bw.Write(feed.Category ?? "");
            bw.Write(feed.TeaserImageUrl ?? "");
            bw.Write("");
            bw.Write("");
            bw.Write("");

            // write DateTime values
            bw.Write(DateTime.MinValue);
            bw.Write(feed.MostRecentEntrance);
            bw.Write(feed.PreviousEntrance);

            bw.Write((int)feed.ArticleViewingType);

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

            bw.Write(newsItem.UtcPublishDateTime);
            bw.Write(newsItem.OriginalDownloadDateTime);
            bw.Write(newsItem.IsFavorite);
            bw.Write(newsItem.HasBeenViewed);
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