using System;
using System.Threading.Tasks;

namespace Common.Azure
{
    public interface IAzureBlobClient
    {
        TimeSpan ReadTimeout { get; set; }
        TimeSpan WriteTimeout { get; set; }
        string ContentType { get; set; }

        Task<T> Get<T>(string blobId);
        Task Save<T>(string blobId, T obj);
        Task Delete(string blobId);
    }
}
