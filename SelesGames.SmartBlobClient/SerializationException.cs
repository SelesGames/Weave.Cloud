using System;

namespace Common.Azure.SmartBlobClient
{
    public class SerializationException : Exception
    {
        public SerializationException(Exception innerException)
            : base(null, innerException)
        { }
    }
}