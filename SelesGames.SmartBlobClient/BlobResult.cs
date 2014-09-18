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

        public bool HasValue { get { return Value is T; } }
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

    public static class BlobResultExtensions
    {
        public static BlobResult<TResult> Copy<T, TResult>(this BlobResult<T> o, Func<T, TResult> map)
        {
            var copy = new BlobResult<TResult>
            {
                BlobName = o.BlobName,
                Content = o.Content,
                SerializationException = o.SerializationException,
                StorageException = o.StorageException,
                Timings = o.Timings,
            };
            if (o.HasValue)
                copy.Value = map(o.Value);
            return copy;
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