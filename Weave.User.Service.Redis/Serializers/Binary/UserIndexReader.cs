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

            userIndex.Id = br.ReadGuid();
            userIndex.LastModified = br.ReadDateTime();
            userIndex.PreviousLoginTime = br.ReadDateTime();
            userIndex.CurrentLoginTime = br.ReadDateTime();
            userIndex.ArticleDeletionTimeForMarkedRead = ReadString();
            userIndex.ArticleDeletionTimeForUnread = ReadString();

            var feedCount = br.ReadInt32();

            for (int i = 0; i < feedCount; i++)
            {
                ReadFeedIndex();
            }
        }

        void ReadFeedIndex()
        {
            feedIndex = new FeedIndex();

            feedIndex.Id = br.ReadGuid(); 
            
            // read string values
            feedIndex.Uri = ReadString();
            feedIndex.Name = ReadString();
            feedIndex.IconUri = ReadString();
            feedIndex.Category = ReadString();
            feedIndex.TeaserImageUri = ReadString();
            object unused = ReadString();
            unused = ReadString();
            unused = ReadString();

            // read DateTime values
            unused = br.ReadDateTime();
            feedIndex.MostRecentEntrance = br.ReadDateTime();
            feedIndex.PreviousEntrance = br.ReadDateTime();

            feedIndex.ArticleViewingType = (ArticleViewingType)br.ReadInt32();

            var newsItemCount = br.ReadInt32();

            for (int i = 0; i < newsItemCount; i++)
            {
                ReadNewsItemIndex();
            }

            userIndex.FeedIndices.Add(feedIndex);
        }

        void ReadNewsItemIndex()
        {
            var newsItemIndex = new NewsItemIndex();

            newsItemIndex.Id = br.ReadGuid();

            newsItemIndex.UtcPublishDateTime = br.ReadDateTime();
            newsItemIndex.OriginalDownloadDateTime = br.ReadDateTime();
            newsItemIndex.IsFavorite = br.ReadBoolean();
            newsItemIndex.HasBeenViewed = br.ReadBoolean();
            newsItemIndex.HasImage = br.ReadBoolean();

            feedIndex.NewsItemIndices.Add(newsItemIndex);
        }

        string ReadString()
        {
            var read = br.ReadString();
            if (read == "")
                return null;
            else
                return read;
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
