using System.Linq;
using Weave.User.BusinessObjects.Mutable;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class UserIndexBinarySerializer : IByteSerializer
    {
        public byte[] WriterUserIndex(UserIndex userIndex)
        {
            using (var helper = new UserIndexWriter(userIndex))
            {
                helper.Write();
                return helper.GetBytes();
            }
        }

        public UserIndex ReadUserIndex(byte[] byteArray)
        {
            using (var helper = new UserIndexReader(byteArray))
            {
                helper.Read();
                return helper.GetUserIndex();
            }
        }

        public T ReadObject<T>(byte[] byteArray)
        {
            return new[] { ReadUserIndex(byteArray) }.Cast<T>().First();
        }

        public byte[] WriteObject<T>(T obj)
        {
            return WriterUserIndex(new[] { obj }.Cast<UserIndex>().First());
            //throw new NotImplementedException();
        }
    }
}