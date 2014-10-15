using StackExchange.Redis;
using System.IO;
using System.Text;
using Weave.User.Service.Redis.Serializers;

namespace Weave.FeedUpdater.Messaging
{
    class FeedUpdateNoticeMap : IRedisValueMap<FeedUpdateNotice>
    {
        public FeedUpdateNotice Map(RedisValue o)
        {
            try
            {
                if (!o.HasValue)
                    return null;

                var bytes = (byte[])o;
                using (var ms = new MemoryStream(bytes))
                using (var br = new BinaryReader(ms, Encoding.UTF8))
                {
                    var notice = new FeedUpdateNotice();

                    notice.RefreshTime = br.ReadDateTime();
                    notice.FeedUri = br.ReadString();

                    return notice;
                }
            }
            catch { }

            return null;
        }

        public RedisValue Map(FeedUpdateNotice update)
        {
            byte[] bytes;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms, Encoding.UTF8))
            {
                bw.Write(update.RefreshTime);
                bw.Write(update.FeedUri);

                bytes = ms.ToArray();
            }

            return bytes;
        }
    }
}