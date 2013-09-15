using Microsoft.ApplicationServer.Caching;
using ProtoBuf;
using System.IO;
using Weave.User.DataStore;

namespace Weave.User.Service.Cache
{
    public class UserNewsItemStateCacheSerializer : IDataCacheObjectSerializer
    {
        public object Deserialize(Stream stream)
        {
            return Serializer.Deserialize<UserNewsItemState>(stream);
        }

        public void Serialize(Stream stream, object value)
        {
            var newsItemState = (UserNewsItemState)value;
            Serializer.Serialize<UserNewsItemState>(stream, newsItemState);
        }
    }
}
