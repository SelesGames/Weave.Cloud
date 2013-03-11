using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace SelesGames.Rest.JsonDotNet
{
    public class StreamToJson<T> : IMapper<Stream, T>
    {
        public Encoding Encoding { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }

        public StreamToJson()
        {
            Encoding = new UTF8Encoding(false, false);
            SerializerSettings = new JsonSerializerSettings();
        }

        public T Map(Stream stream)
        {
            var serializer = JsonSerializer.Create(SerializerSettings);
            using (var streamReader = new StreamReader(stream, Encoding))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }
    }
}
