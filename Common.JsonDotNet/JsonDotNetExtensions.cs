using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Common.JsonDotNet
{
    public static class JsonDotNetExtensions
    {
        public static T ReadObject<T>(this Stream readStream, JsonSerializerSettings serializerSettings, Encoding encoding)
        {
            T result = default(T);
            var serializer = JsonSerializer.Create(serializerSettings);

            using (var streamReader = new StreamReader(readStream, encoding))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                result = serializer.Deserialize<T>(jsonTextReader);
                jsonTextReader.Close();
                streamReader.Close();
            }

            return result;
        }

        public async static Task WriteObject<T>(this Stream writeStream, T obj, JsonSerializerSettings serializerSettings, Encoding encoding)
        {
            var serializer = JsonSerializer.Create(serializerSettings);

            using (var ms = new MemoryStream())
            using (var streamWriter = new StreamWriter(ms, encoding))
            using (var jsonTextWriter = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(jsonTextWriter, obj);
                jsonTextWriter.Flush();

                ms.Position = 0;
                await ms.CopyToAsync(writeStream).ConfigureAwait(false);

                jsonTextWriter.Close();
                streamWriter.Close();
                ms.Close();
            }
        }
    }
}
