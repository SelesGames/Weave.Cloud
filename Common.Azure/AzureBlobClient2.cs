using SelesGames.Common;
using System.IO;
using System.Threading.Tasks;

namespace Common.Azure
{
    internal class AzureBlobClient2<T> : AzureBlobClient<T>
    {
        IStreamHandler streamHandler;

        public AzureBlobClient2(string storageAccountName, string key, string container, bool useHttps, IStreamHandler streamHandler)
            : base(storageAccountName, key, container, useHttps)
        {
            this.streamHandler = streamHandler;
        }

        protected override T ReadObject(Stream stream)
        {
            return streamHandler.ReadObjectFromStream<T>(stream);
        }

        protected async override Task<Stream> CreateStream(T obj)
        {
            var ms = new MemoryStream();
            await streamHandler.WriteObjectToStream(ms, obj);
            ms.Position = 0;
            return ms;
        }
    }
}