using StackExchange.Redis;
using System.IO;
using System.Text;
using Weave.User.Service.Redis.Serializers;

namespace Weave.User.BusinessObjects.Mutable.Cache.Messaging
{
    class UserIndexUpdateNoticeMap : IRedisValueMap<UserIndexUpdateNotice>
    {
        public UserIndexUpdateNotice Map(RedisValue o)
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

        public RedisValue Map(UserIndexUpdateNotice update)
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