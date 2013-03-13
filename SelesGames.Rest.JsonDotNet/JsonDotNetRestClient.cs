using Newtonsoft.Json;
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
            var mapper = new StreamToJson<T> 
            { 
                Encoding = Encoding, 
                SerializerSettings = SerializerSettings 
            };
            return mapper.Map(stream);
        }
    }
}
