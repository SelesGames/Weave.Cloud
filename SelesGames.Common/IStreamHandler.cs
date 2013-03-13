using System.IO;
using System.Threading.Tasks;

namespace SelesGames.Common
{
    public interface IStreamHandler
    {
        Task<T> ReadObjectFromStream<T>(Stream readStream);
        Task WriteObjectToStream<T>(Stream writeStream, T obj);
    }

    public interface IStreamHandler<T>
    {
        Task<T> ReadObjectFromStream(Stream readStream);
        Task WriteObjectToStream(Stream writeStream, T obj);
    }
}
