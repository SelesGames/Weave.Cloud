using Newtonsoft.Json;
using SelesGames.Common;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Common.JsonDotNet
{
    public class JsonDotNetStreamHandler : IStreamHandler
    {
        public Encoding Encoding { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }

        public JsonDotNetStreamHandler()
        {
            Encoding = new UTF8Encoding(false, false);
            SerializerSettings = new JsonSerializerSettings();
        }

        public Task<T> ReadObjectFromStream<T>(Stream readStream)
        {
            return Task.FromResult(readStream.ReadObject<T>(SerializerSettings, Encoding));
        }

        public Task WriteObjectToStream<T>(Stream writeStream, T obj)
        {
            return writeStream.WriteObject(obj, SerializerSettings, Encoding);
        }
    }
}
