using System.IO;

namespace Weave.User.Service.Redis.Serializers.ProtoBuf
{
    class ProtobufSerializerHelper : IByteSerializer
    {
        public T ReadObject<T>(byte[] byteArray)
        {
            T result;

            using (var readStream = new MemoryStream(byteArray))
            {
                result = global::ProtoBuf.Serializer.Deserialize<T>(readStream);
                readStream.Close();
            }

            return result;
        }

        public byte[] WriteObject<T>(T obj)
        {
            byte[] result;


            using (var ms = new MemoryStream())
            {
                global::ProtoBuf.Serializer.Serialize<T>(ms, obj);
                result = ms.ToArray();
                ms.Close();
            }

            return result;
        }
    }
}
