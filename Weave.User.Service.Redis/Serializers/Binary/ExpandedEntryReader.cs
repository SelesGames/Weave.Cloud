using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Weave.Updater.BusinessObjects;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class ExpandedEntryReader : IDisposable
    {
        readonly MemoryStream ms;
        readonly BinaryReader br;

        ExpandedEntry entry;
        IEnumerator<bool> bitEnumerator;

        internal ExpandedEntryReader(byte[] byteArray)
        {
            ms = new MemoryStream(byteArray);
            br = new BinaryReader(ms, Encoding.UTF8);
        }

        internal ExpandedEntry Get()
        {
            return entry;
        }

        internal void Read()
        {
            entry = new ExpandedEntry();

            entry.Id = br.ReadGuid();

            // FeedId is not canonical, and is specific to a particular user's Feed
            //entry.FeedId = br.ReadGuid();
            entry.UtcPublishDateTime = br.ReadDateTime();
            entry.OriginalDownloadDateTime = br.ReadDateTime();

            entry.Title = br.ReadString();
            entry.OriginalPublishDateTimeString = br.ReadString();
            entry.Link = br.ReadString();

            // a byte containing 8 true/false values for the presence of subsequent Strings value
            var stringState = br.ReadByte();
            bitEnumerator = stringState.GetBits().GetEnumerator();

            //if (NextBit()) entry.Description = br.ReadString();
            if (NextBit()) entry.YoutubeId = br.ReadString();
            if (NextBit()) entry.VideoUri = br.ReadString();
            if (NextBit()) entry.PodcastUri = br.ReadString();
            if (NextBit()) entry.ZuneAppId = br.ReadString();
            if (NextBit()) entry.OriginalRssXml = br.ReadString();

            // don't read/record the Image Urls, as this data should be replicated in the Images collection
            //var imageUrlsCount = br.ReadInt32();

            //for (int i = 0; i < imageUrlsCount; i++)
            //{
            //    var imageUrl = br.ReadString();
            //    entry.ImageUrls.Add(imageUrl);
            //}

            var imagesCount = br.ReadInt32();

            for (int i = 0; i < imagesCount; i++)
            {
                ReadImage();
            }
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
