using System;

namespace Weave.User.Service.Redis.Serializers
{
    public class SerializationException : Exception
    {
        public SerializationException(Exception innerException)
            : base(null, innerException)
        { }
    }
}