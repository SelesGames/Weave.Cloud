using System;
using System.IO;
using System.Text;
using Weave.User.BusinessObjects;
using Weave.User.BusinessObjects.Mutable;

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

            userIndex.Id = ReadGuid();
            userIndex.PreviousLoginTime = ReadDateTime();
            userIndex.CurrentLoginTime = ReadDateTime();
            userIndex.ArticleDeletionTimeForMarkedRead = ReadString();
            userIndex.ArticleDeletionTimeForUnread = ReadString();

            var feedCount = br.ReadInt32();

            //userIndex.FeedIndices = new List<FeedIndex>();

            for (int i = 0; i < feedCount; i++)
            {
                ReadFeedIndex();
            }
        }

        void ReadFeedIndex()
        {
            feedIndex = new FeedIndex();

            feedIndex.Id = ReadGuid(); 
            
            // read string values
            feedIndex.Uri = ReadString();
            feedIndex.Name = ReadString();
            feedIndex.IconUri = ReadString();
            feedIndex.Category = ReadString();
            feedIndex.TeaserImageUrl = ReadString();
            feedIndex.Etag = ReadString();
            feedIndex.LastModified = ReadString();
            feedIndex.MostRecentNewsItemPubDate = ReadString();

            // read DateTime values
            feedIndex.LastRefreshedOn = ReadDateTime();
            feedIndex.MostRecentEntrance = ReadDateTime();
            feedIndex.PreviousEntrance = ReadDateTime();

            feedIndex.ArticleViewingType = (ArticleViewingType)br.ReadInt32();

            var newsItemCount = br.ReadInt32();

            //feedIndex.NewsItemIndices = new List<Weave.User.BusinessObjects.Mutable.NewsItemIndex>();

            for (int i = 0; i < newsItemCount; i++)
            {
                ReadNewsItemIndex();
            }

            userIndex.FeedIndices.Add(feedIndex);
        }

        void ReadNewsItemIndex()
        {
            var newsItemIndex = new NewsItemIndex();

            newsItemIndex.Id = ReadGuid();

            newsItemIndex.UrlHash = br.ReadInt64();
            newsItemIndex.TitleHash = br.ReadInt64();
            newsItemIndex.UtcPublishDateTime = ReadDateTime();
            newsItemIndex.OriginalDownloadDateTime = ReadDateTime();
            newsItemIndex.IsFavorite = br.ReadBoolean();
            newsItemIndex.HasBeenViewed = br.ReadBoolean();
            newsItemIndex.HasImage = br.ReadBoolean();

            feedIndex.NewsItemIndices.Add(newsItemIndex);
        }

        Guid ReadGuid()
        {
            return new Guid(br.ReadBytes(16));
        }

        string ReadString()
        {
            var read = br.ReadString();
            if (read == "")
                return null;
            else
                return read;
        }

        DateTime ReadDateTime()
        {
            return DateTime.FromBinary(br.ReadInt64());
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
