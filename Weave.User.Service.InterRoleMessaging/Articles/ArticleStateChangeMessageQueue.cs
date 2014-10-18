using StackExchange.Redis;
using System;
using System.IO;
using Weave.Services.Redis.Ambient;
using Weave.User.Service.Redis;
using Weave.User.Service.Redis.Communication.Generic;
using Weave.User.Service.Redis.Serializers;

namespace Weave.User.Service.InterRoleMessaging.Articles
{
    class ArticleStateChangeNotificationMap : IRedisValueMap<ArticleStateChangeNotification>
    {
        public ArticleStateChangeNotification Map(RedisValue value)
        {
            var o = new ArticleStateChangeNotification();

            using (var ms = new MemoryStream(value))
            using (var br = new BinaryReader(ms))
            {
                o.UserId = new Guid(br.ReadBytes(16));
                o.ArticleId = new Guid(br.ReadBytes(16));
                o.Change = (ArticleStateChange)br.ReadByte();
                o.Source = br.ReadString();
            }

            return o;
        }

        public RedisValue Map(ArticleStateChangeNotification o)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(o.UserId.ToByteArray());
                bw.Write(o.ArticleId.ToByteArray());
                bw.Write((byte)o.Change);
                bw.Write(o.Source ?? "");

                return ms.ToArray();
            }
        }
    }

    public class ArticleStateChangeMessageQueue : MessageQueue<ArticleStateChangeNotification>
    {
        public ArticleStateChangeMessageQueue()
            : base(CreateDatabase(), "articleMQ", "articlePL", new ArticleStateChangeNotificationMap()) 
        { }

        static IDatabaseAsync CreateDatabase()
        {
            return Settings.StandardConnection.GetDatabase(DatabaseNumbers.MESSAGE_QUEUE);
        }
    
        //protected override ArticleStateChangeNotification Map(RedisValue value)
        //{
        //    var o = new ArticleStateChangeNotification();

        //    using (var ms = new MemoryStream(value))
        //    using (var br = new BinaryReader(ms))
        //    {
        //        o.UserId = new Guid(br.ReadBytes(16));
        //        o.ArticleId = new Guid(br.ReadBytes(16));
        //        o.Change = (ArticleStateChange)br.ReadByte();
        //        o.Source = br.ReadString();
        //    }

        //    return o;
        //}

        //protected override RedisValue Map(ArticleStateChangeNotification o)
        //{
        //    using (var ms = new MemoryStream())
        //    using (var bw = new BinaryWriter(ms))
        //    {
        //        bw.Write(o.UserId.ToByteArray());
        //        bw.Write(o.ArticleId.ToByteArray());
        //        bw.Write((byte)o.Change);
        //        bw.Write(o.Source ?? "");

        //        return ms.ToArray();
        //    }
        //}
    }
}