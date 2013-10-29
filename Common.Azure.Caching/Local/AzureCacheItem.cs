using Microsoft.ApplicationServer.Caching;
using System;

namespace Common.Azure.Caching.Local
{
    public class AzureCacheItem
    {
        public bool IsNew { get; set; }
        public DataCacheItemVersion Version { get; private set; }
        public object Value { get; private set; }

        public AzureCacheItem(object val, DataCacheItemVersion version)
        {
            if (val == null) throw new ArgumentNullException("val");
            if (version == null) throw new ArgumentNullException("version");

            Value = val;
            Version = version;
        }
    }
}
