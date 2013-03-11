using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace SelesGames.Rest.StreamMappers
{
    public class StreamToJson<T> : IMapper<Stream, T>
    {
        DataContractJsonSerializer deserializer;

        public StreamToJson(IEnumerable<Type> knownTypes)
        {
            deserializer = new DataContractJsonSerializer(typeof(T), knownTypes);
        }

        public StreamToJson()
        {
            deserializer = new DataContractJsonSerializer(typeof(T));
        }

        public T Map(Stream stream)
        {
            return (T)deserializer.ReadObject(stream);
        }
    }
}
