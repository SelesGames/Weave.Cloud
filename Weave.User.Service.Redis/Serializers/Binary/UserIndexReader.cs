using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Weave.User.Service.Redis.DTOs;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class UserIndexReader : IDisposable
    {
        readonly MemoryStream ms;
        readonly BinaryReader br;

        UserIndex userIndex;
        FeedIndex feedIndex;

        internal UserIndexReader(byte[] byteArray)
        {
            ms = new MemoryStream(byteArray);
            br = new BinaryReader(ms, Encoding.UTF8);
        }

        internal UserIndex GetUserIndex()
        {
            return userIndex;
        }

        internal void Read()
        {
            userIndex = new UserIndex();

            userIndex.Id = new Guid(br.ReadBytes(16));
            userIndex.PreviousLoginTime = DateTime.FromBinary(br.ReadInt64());
            userIndex.CurrentLoginTime = DateTime.FromBinary(br.ReadInt64());
            userIndex.ArticleDeletionTimeForMarkedRead = br.ReadString();
            userIndex.ArticleDeletionTimeForUnread = br.ReadString();

            var feedCount = br.ReadInt32();

            userIndex.FeedIndices = new List<FeedIndex>();

            for (int i = 0; i < feedCount; i++)
            {
                ReadFeedIndex();
            }
        }

        void ReadFeedIndex()
        {
            feedIndex = new FeedIndex();

            feedIndex.Id = new Guid(br.ReadBytes(16)); 
            
            // read string values
            feedIndex.Uri = br.ReadString();
            feedIndex.Name = br.ReadString();
            feedIndex.IconUri = br.ReadString();
            feedIndex.Category = br.ReadString();
            feedIndex.TeaserImageUrl = br.ReadString();
            feedIndex.Etag = br.ReadString();
            feedIndex.LastModified = br.ReadString();
            feedIndex.MostRecentNewsItemPubDate = br.ReadString();

            // read DateTime values
            feedIndex.LastRefreshedOn = DateTime.FromBinary(br.ReadInt64());
            feedIndex.MostRecentEntrance = DateTime.FromBinary(br.ReadInt64());
            feedIndex.PreviousEntrance = DateTime.FromBinary(br.ReadInt64());

            feedIndex.ArticleViewingType = (Weave.User.BusinessObjects.ArticleViewingType)br.ReadInt32();

            var newsItemCount = br.ReadInt32();

            feedIndex.NewsItemIndices = new List<Weave.User.BusinessObjects.Mutable.NewsItemIndex>();

            for (int i = 0; i < newsItemCount; i++)
            {
                ReadNewsItemIndex();
            }

            userIndex.FeedIndices.Add(feedIndex);
        }

        void ReadNewsItemIndex()
        {
            var newsItemIndex = new BusinessObjects.Mutable.NewsItemIndex();

            newsItemIndex.Id = new Guid(br.ReadBytes(16));

            newsItemIndex.UrlHash = br.ReadInt64();
            newsItemIndex.TitleHash = br.ReadInt64();
            newsItemIndex.UtcPublishDateTime = DateTime.FromBinary(br.ReadInt64());
            newsItemIndex.OriginalDownloadDateTime = DateTime.FromBinary(br.ReadInt64());
            newsItemIndex.IsFavorite = br.ReadBoolean();
            newsItemIndex.HasBeenViewed = br.ReadBoolean();
            newsItemIndex.HasImage = br.ReadBoolean();

            feedIndex.NewsItemIndices.Add(newsItemIndex);
        }

        public void Dispose()
        {
            br.Close();
            ms.Close();
            br.Dispose();
            ms.Dispose();
        }
    }
}
