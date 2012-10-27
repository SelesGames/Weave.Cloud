using System;
using System.IO;

namespace SelesGames.Rest
{
    public class DelegateRestClient<T> : RestClient<T>
    {
        Func<Stream, T> map;

        public DelegateRestClient(Func<Stream, T> map)
        {
            this.map = map;
        }

        protected override T ReadObject(Stream stream)
        {
            return map(stream);
        }
    }

    public static class RestClient
    {
        public static DelegateRestClient<T> Create<T>(Func<Stream, T> map, bool useGzip = false)
        {
            return new DelegateRestClient<T>(map) { UseGzip = useGzip };
        }
    }
}
