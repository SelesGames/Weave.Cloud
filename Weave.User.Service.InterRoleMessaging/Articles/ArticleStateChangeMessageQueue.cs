using StackExchange.Redis;
using System;
using System.IO;
using Weave.User.Service.Redis.Communication.Generic;

namespace Weave.User.Service.InterRoleMessaging.Articles
{
    public class ArticleStateChangeMessageQueue : MessageQueue<ArticleStateChangeNotification>
    {
        public ArticleStateChangeMessageQueue(ConnectionMultiplexer multiplexer)
            : base(multiplexer.GetDatabase(9), "articleMQ", "articlePL") { }
    
        protected override ArticleStateChangeNotification Map(RedisValue value)
        {
            var o = new ArticleStateChangeNotification();

            using (var ms = new MemoryStream(value))
            using (var br = new BinaryReader(ms))
            {
                o.UserId = new Guid(br.ReadBytes(16));
                o.ArticleId = new Guid(br.ReadBytes(16));
                o.Change = (ArticleStateChange)br.ReadByte();
            }

            return o;
        }

        protected override RedisValue Map(ArticleStateChangeNotification o)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(o.UserId.ToByteArray());
                bw.Write(o.ArticleId.ToByteArray());
                bw.Write((byte)o.Change);

                return ms.ToArray();
            }
        }
    }
}
