using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace SelesGames.WebApi.Protobuf
{
    public class ProtobufFormatter : MediaTypeFormatter
    {
        Dictionary<Type, MethodInfo> cachedGenericMethods = new Dictionary<Type, MethodInfo>();
        object sync = new object();
        MethodInfo methodInfo;

        public ProtobufFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/protobuf"));

            var serializeMethods = typeof(Serializer).GetMethods().Where(o => o.Name == "Serialize").Select(o => new { MethodInfo = o, Parameters = o.GetParameters() }).ToList();
            methodInfo = serializeMethods.Single(o => o.Parameters.Any(x => x.ParameterType == typeof(Stream))).MethodInfo;
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
            if (type == null) throw new ArgumentNullException("type");
            if (readStream == null) throw new ArgumentNullException("readStream");

            // Create task reading the content
            return Task.Factory.StartNew(() =>
            {
                return Serializer.NonGeneric.Deserialize(type, readStream);
            });
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (writeStream == null) throw new ArgumentNullException("writeStream");

            MethodInfo serialize;

            if (cachedGenericMethods.ContainsKey(type))
                serialize = cachedGenericMethods[type];
            else
                serialize = AddMethodForType(type);

            // Create task writing the serialized content
            return Task.Factory.StartNew(() =>
            {
                serialize.Invoke(null, new[] { writeStream, value });
                //Serializer.NonGeneric.SerializeWithLengthPrefix(writeStream, value, PrefixStyle.Base128,  
            });
        }

        MethodInfo AddMethodForType(Type type)
        {
            var serialize = methodInfo.MakeGenericMethod(type);

            lock (sync)
            {
                if (!cachedGenericMethods.ContainsKey(type))
                    cachedGenericMethods.Add(type, serialize);
                else
                    serialize = cachedGenericMethods[type];
            }

            return serialize;
        }
    }
}