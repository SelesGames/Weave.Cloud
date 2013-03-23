using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace SelesGames.Rest
{
    public class JsonRestClient : RestClient
    {
        IEnumerable<Type> knownTypes;

        public JsonRestClient()
        {
            Headers.Accept = "application/json";
            Headers.ContentType = "application/json";
        }

        public JsonRestClient(IEnumerable<Type> knownTypes)
            : this()
        {
            this.knownTypes = knownTypes;
        }

        protected override T ReadObject<T>(Stream readStream)
        {
            var deserializer = new DataContractJsonSerializer(typeof(T), knownTypes);
            return (T)deserializer.ReadObject(readStream);
        }

        protected override void WriteObject<T>(Stream writeStream, T obj)
        {
            var serializer = new DataContractJsonSerializer(typeof(T), knownTypes);
            serializer.WriteObject(writeStream, obj);
        }
    }
}