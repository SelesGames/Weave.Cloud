using System;
using System.IO;
using System.Text;
using Weave.User.BusinessObjects.Mutable;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class CanonicalFeedIndexReader : IDisposable
    {
        readonly MemoryStream ms;
        readonly BinaryReader br;

        FeedIndex feedIndex;

        internal CanonicalFeedIndexReader(byte[] byteArray)
        {
            ms = new MemoryStream(byteArray);
            br = new BinaryReader(ms, Encoding.UTF8);
        }

        internal FeedIndex GetCanonicalFeedIndex()
        {
            return feedIndex;
        }

        public void Read()
        {
            feedIndex = new FeedIndex();

            feedIndex.Id = br.ReadGuid();

            // CERTAIN VALUES NO LONGER HOLD TRUE FOR CANONICAL FEEDS
            // since this feed data is not specific to any particular user, the
            // following values will be omitted:
            //feedIndex.Name = ReadString();
            //feedIndex.Category = ReadString();
            //feedIndex.MostRecentEntrance = ReadDateTime();
            //feedIndex.PreviousEntrance = ReadDateTime();
            //feedIndex.ArticleViewingType = (ArticleViewingType)br.ReadInt32();
            

            // read string values
            feedIndex.Uri = ReadString();
            feedIndex.IconUri = ReadString();
            feedIndex.TeaserImageUrl = ReadString();
            feedIndex.Etag = ReadString();
            feedIndex.LastModified = ReadString();
            feedIndex.MostRecentNewsItemPubDate = ReadString();

            // read DateTime values
            feedIndex.LastRefreshedOn = br.ReadDateTime();

            var newsItemCount = br.ReadInt32();

            for (int i = 0; i < newsItemCount; i++)
            {
                ReadNewsItemIndex();
            }
        }

        void ReadNewsItemIndex()
        {
            var newsItemIndex = new NewsItemIndex();

            newsItemIndex.Id = br.ReadGuid();

            // CERTAIN VALUES NO LONGER HOLD TRUE FOR CANONICAL NEWS ITEM INDICES
            // since this news item data is not specific to any particular user, the
            // following values will be omitted:
            //newsItemIndex.IsFavorite = br.ReadBoolean();
            //newsItemIndex.HasBeenViewed = br.ReadBoolean();

            newsItemIndex.UtcPublishDateTime = br.ReadDateTime();
            newsItemIndex.OriginalDownloadDateTime = br.ReadDateTime();
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
