using System;
using System.IO;
using Weave.User.Service.Redis.DTOs;

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
            bw = new BinaryWriter(ms);
        }

        internal byte[] GetBytes()
        {
            return ms.ToArray();
        }

        internal void Write()
        {
            bw.Write(user.Id.ToByteArray());
            bw.Write(user.PreviousLoginTime.ToBinary());
            bw.Write(user.CurrentLoginTime.ToBinary());
            bw.Write(user.ArticleDeletionTimeForMarkedRead);
            bw.Write(user.ArticleDeletionTimeForUnread);

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
            bw.Write(feed.Id.ToByteArray());

            // write string values
            bw.Write(feed.Uri);
            bw.Write(feed.Name);
            bw.Write(feed.IconUri);
            bw.Write(feed.Category);
            bw.Write(feed.TeaserImageUrl);
            bw.Write(feed.Etag);
            bw.Write(feed.LastModified);
            bw.Write(feed.MostRecentNewsItemPubDate);

            // write DateTime values
            bw.Write(feed.LastRefreshedOn.ToBinary());
            bw.Write(feed.MostRecentEntrance.ToBinary());
            bw.Write(feed.PreviousEntrance.ToBinary());

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

        void WriteNewsItemIndex(Weave.User.BusinessObjects.Mutable.NewsItemIndex newsItem)
        {
            bw.Write(newsItem.Id.ToByteArray());

            bw.Write(newsItem.UrlHash);
            bw.Write(newsItem.TitleHash);
            bw.Write(newsItem.UtcPublishDateTime.ToBinary());
            bw.Write(newsItem.OriginalDownloadDateTime.ToBinary());
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