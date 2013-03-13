using Common.JsonDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Common.Azure
{
    public class AzureJsonDotNetBlobClient<T> : AzureBlobClient<T>, IAzureBlobClient<T>
    {
        public Encoding Encoding { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }

        public AzureJsonDotNetBlobClient(string storageAccountName, string key, string container, bool useHttps)
            : base(storageAccountName, key, container, useHttps)
        {
            ContentType = "application/json; charset=utf-8";

            Encoding = new UTF8Encoding(false, false);
            SerializerSettings = new JsonSerializerSettings();
            SerializerSettings.Converters.Add(new IsoDateTimeConverter());
        }




        #region JSON Serialization helpers

        protected override T ReadObject(System.IO.Stream stream)
        {
            return stream.ReadObject<T>(SerializerSettings, Encoding);

            //using (var streamReader = new StreamReader(stream, Encoding))
            //using (var jsonTextReader = new JsonTextReader(streamReader))
            //{
            //    var serializer = JsonSerializer.Create(SerializerSettings);
            //    return serializer.Deserialize<T>(jsonTextReader);
            //}
        }

        protected override async Task<Stream> CreateStream(T obj)
        {
            var ms = new MemoryStream();
            await ms.WriteObject(obj, SerializerSettings, Encoding);
            return ms;
            //var returnStream = new MemoryStream();

            //using (var ms = new MemoryStream())
            //using (var streamWriter = new StreamWriter(ms, Encoding))
            //using (var jsonTextWriter = new JsonTextWriter(streamWriter))
            //{
            //    var serializer = JsonSerializer.Create(SerializerSettings);
            //    serializer.Serialize(jsonTextWriter, obj);
            //    jsonTextWriter.Flush();
            //    ms.Position = 0;
            //    await ms.CopyToAsync(returnStream);
            //}

            //returnStream.Position = 0;
            //return returnStream;
        }

        //int bufferSize = 1024 * 1024;

        //void WriteObject(Stream stream, T obj)
        //{
        //    //using (var streamWriter = new StreamWriter(stream, Encoding, bufferSize, true))
        //    using (var streamWriter = new StreamWriter(stream, Encoding))
        //    using (var jsonTextWriter = new JsonTextWriter(streamWriter))
        //    {
        //        var serializer = JsonSerializer.Create(SerializerSettings);
        //        serializer.Serialize(jsonTextWriter, obj);
        //        jsonTextWriter.Flush();
        //    }
        //}

        #endregion
    }
}