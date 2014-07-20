using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Weave.User.Service.Redis.Serializers.Json
{
    class JsonSerializerHelper : IByteSerializer
    {
        Encoding encoding = Encoding.UTF8;
        JsonSerializerSettings serializerSettings = new JsonSerializerSettings 
        { 
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        public T ReadObject<T>(byte[] byteArray)
        {
            T result;
            var serializer = JsonSerializer.Create(serializerSettings);

            using (var readStream = new MemoryStream(byteArray))
            using (var streamReader = new StreamReader(readStream, encoding))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                result = serializer.Deserialize<T>(jsonTextReader);
                jsonTextReader.Close();
                streamReader.Close();
            }

            return result;
        }

        public byte[] WriteObject<T>(T obj)
        {
            byte[] result;

            var serializer = JsonSerializer.Create(serializerSettings);

            using (var ms = new MemoryStream())
            using (var streamWriter = new StreamWriter(ms, encoding))
            using (var jsonTextWriter = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(jsonTextWriter, obj);
                jsonTextWriter.Flush();

                result = ms.ToArray();

                jsonTextWriter.Close();
                streamWriter.Close();
                ms.Close();
            }

            return result;
        }
    }
}
