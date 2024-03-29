﻿using Microsoft.ApplicationServer.Caching;
using ProtoBuf;
using System.IO;
//using System.IO.Compression;
using Weave.User.DataStore;

namespace Weave.User.Service.Cache
{
    public class UserInfoCacheSerializer : IDataCacheObjectSerializer
    {
        public object Deserialize(Stream stream)
        {
            //using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress, true))
            //{
            //    return Serializer.Deserialize<UserInfo>(gzipStream);
            //}

            return Serializer.Deserialize<UserInfo>(stream);
        }

        public void Serialize(Stream stream, object value)
        {
            //using (var gzipStream = new GZipStream(stream, CompressionMode.Compress, true))
            //{
            //    var user = (UserInfo)value;
            //    Serializer.Serialize<UserInfo>(gzipStream, user);
            //}
            Serializer.Serialize<UserInfo>(stream, (UserInfo)value);
        }
    }
}