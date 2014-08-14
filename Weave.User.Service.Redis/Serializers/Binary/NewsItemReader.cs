using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Weave.User.Service.Redis.DTOs;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class NewsItemReader : IDisposable
    {
        readonly MemoryStream ms;
        readonly BinaryReader br;

        NewsItem newsItem;
        IEnumerator<bool> bitEnumerator;

        internal NewsItemReader(byte[] byteArray)
        {
            ms = new MemoryStream(byteArray);
            br = new BinaryReader(ms, Encoding.UTF8);
        }

        internal NewsItem GetNewsItem()
        {
            return newsItem;
        }

        internal void Read()
        {
            newsItem = new NewsItem();

            newsItem.Id = br.ReadGuid();
            newsItem.UtcPublishDateTime = br.ReadDateTime();
            newsItem.UtcPublishDateTimeString = ReadString();

            // a byte containing 8 true/false values for the presence of subsequent Strings and Image value
            var stringState = br.ReadByte();
            bitEnumerator = stringState.GetBits().GetEnumerator();

            if (NextBit()) newsItem.Title = br.ReadString();
            if (NextBit()) newsItem.Link = br.ReadString();
            if (NextBit()) newsItem.ImageUrl = br.ReadString();
            if (NextBit()) newsItem.YoutubeId = br.ReadString();
            if (NextBit()) newsItem.VideoUri = br.ReadString();
            if (NextBit()) newsItem.PodcastUri = br.ReadString();
            if (NextBit()) newsItem.ZuneAppId = br.ReadString();
            if (NextBit()) ReadImage();
        }

        void ReadImage()
        {
            var image = new Image();

            image.Width = br.ReadInt32();
            image.Height = br.ReadInt32();

            // read the next 3 strings safely
            image.OriginalUrl = ReadString();
            image.BaseImageUrl = ReadString();
            image.SupportedFormats = ReadString();

            newsItem.Image = image;
        }

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

        public void Dispose()
        {
            br.Close();
            ms.Close();
            br.Dispose();
            ms.Dispose();
        }
    }
}
