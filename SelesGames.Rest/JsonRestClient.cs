using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace SelesGames.Rest
{
    public class JsonRestClient<T> : RestClient<T>
    {
        DataContractJsonSerializer deserializer;

        public JsonRestClient(IEnumerable<Type> knownTypes)
        {
            deserializer = new DataContractJsonSerializer(typeof(T), knownTypes);
        }

        public JsonRestClient()
        {
            deserializer = new DataContractJsonSerializer(typeof(T));
        }

        protected override T ReadObject(Stream stream)
        {
            return (T)deserializer.ReadObject(stream);
        }
    }
}
