using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.User.Service.Redis.DTOs;

namespace Weave.User.Service.Redis.Serializers.Binary
{
    class UserIndexBinarySerializer : IByteSerializer
    {
        public byte[] WriteObject(UserIndex userIndex)
        {
            using (var helper = new UserIndexWriter(userIndex))
            {
                helper.Write();
                return helper.GetBytes();
            }
        }

        public T ReadObject<T>(byte[] byteArray)
        {
            throw new NotImplementedException();
        }

        public byte[] WriteObject<T>(T obj)
        {
            throw new NotImplementedException();
        }
    }
}
