using System;
using System.Collections.Generic;
using System.Text;

namespace Weave.User.Service.Redis
{
    /// <summary>
    /// Thread-safe class for creating Redis keys that are prepended by some string value
    /// </summary>
    public class RedisPrependKeyCreator
    {
        readonly Dictionary<string, byte[]> mappings;
        readonly Encoding encoding;
        readonly object sync = new object();

        static readonly RedisPrependKeyCreator instance = new RedisPrependKeyCreator();

        public RedisPrependKeyCreator()
        {
            mappings = new Dictionary<string, byte[]>();
            encoding = Encoding.ASCII;
        }

        public static byte[] Create(string prepend, byte[] key)
        {
            return instance.CreateKey(prepend, key);
        }

        public byte[] CreateKey(string prepend, byte[] key)
        {
            byte[] prependBytes;
            if (!mappings.TryGetValue(prepend, out prependBytes))
            {
                prependBytes = encoding.GetBytes(prepend);
                Add(prepend, prependBytes);
            }

            var resultKey = new byte[prependBytes.Length + key.Length];

            Buffer.BlockCopy(prependBytes, 0, resultKey, 0, prependBytes.Length);
            Buffer.BlockCopy(key, 0, resultKey, prependBytes.Length, key.Length);

            return resultKey;
        }

        void Add(string val, byte[] bytes)
        {
            lock (sync)
            {
                if (!mappings.ContainsKey(val))
                    mappings.Add(val, bytes);
            }
        }
    }
}