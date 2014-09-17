using StackExchange.Redis;
using System.IO;
using System.Text;

namespace Weave.User.BusinessObjects.Mutable.Cache.PubSub
{
    static class FeedUpdateNoticeExtensions
    {
        public static UserIndexUpdateNotice ReadUserIndexUpdateNotice(this RedisValue o)
        {
            try
            {
                if (!o.HasValue)
                    return null;

                var bytes = (byte[])o;
                using (var ms = new MemoryStream(bytes))
                using (var br = new BinaryReader(ms, Encoding.UTF8))
                {
                    var notice = new UserIndexUpdateNotice();
                    notice.UserId = br.ReadGuid();
                    notice.CacheId = br.ReadGuid();

                    return notice;
                }
            }
            catch { }

            return null;
        }

        public static byte[] WriteToBytes(this UserIndexUpdateNotice update)
        {
            byte[] bytes;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms, Encoding.UTF8))
            {
                bw.Write(update.UserId);
                bw.Write(update.CacheId);

                bytes = ms.ToArray();
            }

            return bytes;
        }
    }
}