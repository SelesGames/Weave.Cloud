using ProtoBuf;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SelesGames.WebApi.Protobuf
{
    public class ProtobufFormatter : MediaTypeFormatter
    {
        public ProtobufFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/protobuf"));
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            // Create task reading the content
            return Task.Factory.StartNew(() =>
            {
                return Serializer.NonGeneric.Deserialize(type, readStream);
            });
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            // Create task writing the serialized content
            return Task.Factory.StartNew(() =>
            {
                Serializer.NonGeneric.Serialize(writeStream, value);
            });
        }
    }
}