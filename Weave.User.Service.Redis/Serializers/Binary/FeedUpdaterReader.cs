using System;
using System.IO;
using System.Text;
using Weave.Updater.BusinessObjects;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class FeedUpdaterReader : IDisposable
    {
        readonly MemoryStream ms;
        readonly BinaryReader br;

        Feed feed;
        NewsItemRecord record;

        internal FeedUpdaterReader(byte[] byteArray)
        {
            ms = new MemoryStream(byteArray);
            br = new BinaryReader(ms, Encoding.UTF8);
        }

        internal Feed Get()
        {
            return feed;
        }

        internal void Read()
        {
            feed = new Feed("unused");

            feed.LastRefreshedOn = br.ReadDateTime();

            // read string values
            feed.IconUri = ReadString();
            feed.TeaserImageUri = ReadString();
            feed.Etag = ReadString();
            feed.LastModified = ReadString();
            feed.MostRecentNewsItemPubDate = ReadString();

            var recordCount = br.ReadInt32();

            for (int i = 0; i < recordCount; i++)
            {
                ReadRecord();
            }
        }

        void ReadRecord()
        {
            record = new NewsItemRecord();

            record.Id = br.ReadGuid();
            record.UtcPublishDateTime = br.ReadDateTime();
            record.Title = br.ReadString();
            record.Link = br.ReadString();

            // ?
            //record.OriginalDownloadDateTime = br.ReadDateTime();
            record.HasImage = br.ReadBoolean();

            feed.News.Add(record);
        }




        #region helper functions

        string ReadString()
        {
            var read = br.ReadString();
            if (read == "")
                return null;
            else
                return read;
        }

        #endregion




        public void Dispose()
        {
            br.Close();
            ms.Close();
            br.Dispose();
            ms.Dispose();
        }
    }
}