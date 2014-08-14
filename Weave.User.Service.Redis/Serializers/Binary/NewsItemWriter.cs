using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Weave.User.Service.Redis.DTOs;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    public class NewsItemWriter : IDisposable
    {
        readonly MemoryStream ms;
        readonly BinaryWriter bw;
        readonly NewsItem newsItem;

        IEnumerator<bool> bitEnumerator;

        internal NewsItemWriter(NewsItem newsItem)
        {
            this.newsItem = newsItem;
            ms = new MemoryStream();
            bw = new BinaryWriter(ms, Encoding.UTF8);
        }

        internal byte[] GetBytes()
        {
            return ms.ToArray();
        }

        internal void Write()
        {
            bw.Write(newsItem.Id);
            bw.Write(newsItem.UtcPublishDateTime);
            bw.Write(newsItem.UtcPublishDateTimeString ?? "");

            var nullStates = 
                new List<object>
                {
                    newsItem.Title,
                    newsItem.Link,
                    newsItem.ImageUrl,
                    newsItem.YoutubeId,
                    newsItem.VideoUri,
                    newsItem.PodcastUri,
                    newsItem.ZuneAppId,
                    newsItem.Image,
                }
                .Select(IsNotNull)
                .ToList();
            var stringState = nullStates.ToByte();
            bitEnumerator = nullStates.GetEnumerator();

            bw.Write(stringState);

            if (NextBit()) bw.Write(newsItem.Title);
            if (NextBit()) bw.Write(newsItem.Link);
            if (NextBit()) bw.Write(newsItem.ImageUrl);
            if (NextBit()) bw.Write(newsItem.YoutubeId);
            if (NextBit()) bw.Write(newsItem.VideoUri);
            if (NextBit()) bw.Write(newsItem.PodcastUri);
            if (NextBit()) bw.Write(newsItem.ZuneAppId);
            if (NextBit()) WriteImage();
        }

        void WriteImage()
        {
            var image = newsItem.Image;

            bw.Write(image.Width);
            bw.Write(image.Height);

            bw.Write(image.OriginalUrl ?? "");
            bw.Write(image.BaseImageUrl ?? "");
            bw.Write(image.SupportedFormats ?? "");
        }

        bool IsNotNull(object o)
        {
            if (o == null)
                return false;

            else
            {
                if (o is string)
                {
                    return !string.IsNullOrWhiteSpace((string)o);
                }
            }

            return true;
        }

        bool NextBit()
        {
            bitEnumerator.MoveNext();
            return bitEnumerator.Current;
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
