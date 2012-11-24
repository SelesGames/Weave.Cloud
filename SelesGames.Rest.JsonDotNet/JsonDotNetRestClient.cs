using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace SelesGames.Rest.JsonDotNet
{
    public class JsonDotNetRestClient<T> : RestClient<T>
    {
        public Encoding Encoding { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }

        public JsonDotNetRestClient()
        {
            Encoding = new UTF8Encoding(false, false);
            SerializerSettings = new JsonSerializerSettings();
        }

        protected override T ReadObject(System.IO.Stream stream)
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
