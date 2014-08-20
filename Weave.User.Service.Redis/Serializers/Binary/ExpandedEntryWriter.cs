using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Weave.Updater.BusinessObjects;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    public class ExpandedEntryWriter : IDisposable
    {
        readonly MemoryStream ms;
        readonly BinaryWriter bw;
        readonly ExpandedEntry entry;

        IEnumerator<bool> bitEnumerator;

        internal ExpandedEntryWriter(ExpandedEntry entry)
        {
            this.entry = entry;
            ms = new MemoryStream();
            bw = new BinaryWriter(ms, Encoding.UTF8);
        }

        internal byte[] GetBytes()
        {
            return ms.ToArray();
        }

        internal void Write()
        {
            bw.Write(entry.Id);
            // FeedId is not canonical, and is specific to a particular user's Feed
            //bw.Write(entry.FeedId);
            bw.Write(entry.UtcPublishDateTime);
            bw.Write(entry.OriginalDownloadDateTime);

            bw.Write(entry.Title);
            bw.Write(entry.OriginalPublishDateTimeString);
            bw.Write(entry.Link);

            var nullStates =
                new List<object>
                {
                    //entry.Description,
                    entry.YoutubeId,
                    entry.VideoUri,
                    entry.PodcastUri,
                    entry.ZuneAppId,
                    entry.OriginalRssXml,
                }
                .Select(IsNotNull)
                .ToList();
            var stringState = nullStates.ToByte();
            bitEnumerator = nullStates.GetEnumerator();

            bw.Write(stringState);

            //if (NextBit()) bw.Write(entry.Description);
            if (NextBit()) bw.Write(entry.YoutubeId);
            if (NextBit()) bw.Write(entry.VideoUri);
            if (NextBit()) bw.Write(entry.PodcastUri);
            if (NextBit()) bw.Write(entry.ZuneAppId);
            if (NextBit()) bw.Write(entry.OriginalRssXml);

            // don't read/record the Image Urls, as this data should be replicated in the Images collection
            //// write the number of ImageUrls
            //bw.Write(entry.ImageUrls.Count());

            //foreach (var imageUrl in entry.ImageUrls)
            //{
            //    bw.Write(imageUrl);
            //}

            // write the number of Images
            bw.Write(entry.Images.Count());

            foreach (var image in entry.Images)
            {
                WriteImage(image);
            }
        }

        void WriteImage(Image image)
        {
            bw.Write(image.Width);
            bw.Write(image.Height);
            bw.Write(image.ContentLength);
            bw.Write(image.Url);

            // optional string values
            bw.Write(image.Format ?? "");
            bw.Write(image.ContentType ?? "");
        }




        #region helper methods

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

        #endregion




        public void Dispose()
        {
            bw.Close();
            ms.Close();
            bw.Dispose();
            ms.Dispose();
        }
    }
}