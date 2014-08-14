using System;
using System.Collections.Generic;
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
        ExpandedEntry entry;
        IEnumerator<bool> bitEnumerator;

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
            feed = new Feed("unused", "unused", null, null);

            feed.LastRefreshedOn = br.ReadDateTime();

            // read string values
            feed.TeaserImageUrl = ReadString();
            feed.Etag = ReadString();
            feed.LastModified = ReadString();
            feed.MostRecentNewsItemPubDate = ReadString();

            var entryCount = br.ReadInt32();

            for (int i = 0; i < entryCount; i++)
            {
                ReadEntry();
            }
        }

        void ReadEntry()
        {
            entry = new ExpandedEntry();

            entry.Id = br.ReadGuid();
            entry.FeedId = br.ReadGuid();
            entry.UtcPublishDateTime = br.ReadDateTime();
            entry.OriginalDownloadDateTime = br.ReadDateTime();

            entry.Title = br.ReadString();
            entry.OriginalPublishDateTimeString = br.ReadString();
            entry.Link = br.ReadString();

            // a byte containing 8 true/false values for the presence of subsequent Strings value
            var stringState = br.ReadByte();
            bitEnumerator = stringState.GetBits().GetEnumerator();

            if (NextBit()) entry.Description = br.ReadString();
            if (NextBit()) entry.YoutubeId = br.ReadString();
            if (NextBit()) entry.VideoUri = br.ReadString();
            if (NextBit()) entry.PodcastUri = br.ReadString();
            if (NextBit()) entry.ZuneAppId = br.ReadString();
            if (NextBit()) entry.OriginalRssXml = br.ReadString();

            var imageUrlsCount = br.ReadInt32();

            for (int i = 0; i < imageUrlsCount; i++)
            {
                var imageUrl = br.ReadString();
                entry.ImageUrls.Add(imageUrl);
            }

            var imagesCount = br.ReadInt32();

            for (int i = 0; i < imagesCount; i++)
            {
                ReadImage();
            }

            feed.Entries.Add(entry);
        }

        void ReadImage()
        {
            var image = new Image();

            image.Width = br.ReadInt32();
            image.Height = br.ReadInt32();
            image.ContentLength = br.ReadInt64();
            image.Url = br.ReadString();

            // optional string values
            image.Format = ReadString();
            image.ContentType = ReadString();

            entry.Images.Add(image);
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

        bool NextBit()
        {
            bitEnumerator.MoveNext();
            return bitEnumerator.Current;
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