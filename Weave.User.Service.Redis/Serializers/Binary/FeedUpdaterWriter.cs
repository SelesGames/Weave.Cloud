using System;
using System.IO;
using System.Linq;
using System.Text;
using Weave.Updater.BusinessObjects;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    public class FeedUpdaterWriter : IDisposable
    {
        readonly MemoryStream ms;
        readonly BinaryWriter bw;
        readonly Feed feed;

        internal FeedUpdaterWriter(Feed feed)
        {
            this.feed = feed;
            ms = new MemoryStream();
            bw = new BinaryWriter(ms, Encoding.UTF8);
        }

        internal byte[] GetBytes()
        {
            return ms.ToArray();
        }

        internal void Write()
        {
            bw.Write(feed.LastRefreshedOn);

            // write string values
            bw.Write(feed.IconUri ?? "");
            bw.Write(feed.TeaserImageUri ?? "");
            bw.Write(feed.Etag ?? "");
            bw.Write(feed.LastModified ?? "");
            bw.Write(feed.MostRecentNewsItemPubDate ?? "");

            // write the number of entries
            bw.Write(feed.News.Count());

            foreach(var record in feed.News)
            {
                Write(record);
            }           
        }

        void Write(NewsItemRecord record)
        {
            bw.Write(record.Id);
            bw.Write(record.UtcPublishDateTime);
            bw.Write(record.Title);
            bw.Write(record.Link);

            // ?
            //bw.Write(record.OriginalDownloadDateTime);
            bw.Write(record.HasImage);
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
