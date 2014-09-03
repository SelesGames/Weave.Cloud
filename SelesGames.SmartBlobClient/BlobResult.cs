using Common.Azure.Blob;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;

namespace Common.Azure.SmartBlobClient
{
    public class BlobResult<T>
    {
        public string BlobName { get; set; }
        public BlobContent Content { get; set; }
        public T Value { get; set; }
        public StorageException StorageException { get; set; }
        public SerializationException SerializationException { get; set; }

        public bool HasValue { get { return Content.Content != null && Value is T; } }
        public int ByteLength { get { return GetByteLength(Content); } }

        public BlobTimings Timings { get; set; }

        static int GetByteLength(BlobContent content)
        {
            if (content.Content != null && content.Content.CanSeek)
                return (int)content.Content.Length;
            else
                return -1;
        }
    }

    //public class RedisWriteResult<T>
    //{
    //    public RedisKey RedisKey { get; set; }
    //    public RedisValue RedisValue { get; set; }

    //    public T ResultValue { get; set; }

    //    public BlobTimings Timings { get; set; }
    //}

    public class BlobTimings
    {
        public TimeSpan ServiceTime { get; set; }
        public TimeSpan SerializationTime { get; set; }

        public static BlobTimings Empty = new BlobTimings { SerializationTime = TimeSpan.Zero, ServiceTime = TimeSpan.Zero };
    }
}