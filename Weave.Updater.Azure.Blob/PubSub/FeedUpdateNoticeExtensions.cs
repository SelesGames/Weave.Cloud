using StackExchange.Redis;
using System;
using System.IO;

namespace Weave.Updater.PubSub
{
    static class FeedUpdateNoticeExtensions
    {
        public static FeedUpdateNotice ReadFeedUpdateNotice(this RedisValue o)
        {
            try
            {
                if (!o.HasValue)
                    return null;

                var bytes = (byte[])o;
                using (var ms = new MemoryStream(bytes))
                using (var br = new BinaryReader(ms))
                {
                    var notice = new FeedUpdateNotice();

                    notice.RefreshTime = DateTime.FromBinary(br.ReadInt64());
                    notice.FeedUri = br.ReadString();

                    return notice;
                }
            }
            catch { }

            return null;
        }

        public static byte[] WriteToBytes(this FeedUpdateNotice update)
        {
            byte[] bytes;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(update.RefreshTime.ToBinary());
                bw.Write(update.FeedUri);

                bytes = ms.ToArray();
            }

            return bytes;
        }
    }
}