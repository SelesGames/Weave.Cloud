using System.Threading.Tasks;

namespace Common.Azure.Blob.Contracts
{
    public interface IBlobRepository
    {
        Task<T> Get<T>(string containerName, string blobName, RequestProperties properties = null);
        Task Save<T>(string containerName, string blobName, T obj, WriteRequestProperties properties = null);
    }
}
