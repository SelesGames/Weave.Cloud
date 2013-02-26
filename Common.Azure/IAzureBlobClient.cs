using System;
using System.Threading.Tasks;

namespace Common.Azure
{
    public interface IAzureBlobClient<T>
    {
        TimeSpan ReadTimeout { get; set; }
        TimeSpan WriteTimeout { get; set; }
        string ContentType { get; set; }

        Task<T> Get(string blobId);
        Task Save(string blobId, T obj);
        Task Delete(string blobId);
    }
}
