using System;

namespace Common.Azure.Caching.Local
{
    public class AzureCacheMissException : Exception
    {
        public string Key { get; private set; }

        public AzureCacheMissException(string key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return string.Format("{0} not found", Key);
        }
    }
}
