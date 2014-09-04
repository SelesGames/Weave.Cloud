using Common.Azure.Blob;
using Common.Azure.SmartBlobClient;
using System;
using System.Threading.Tasks;

namespace Weave.User.BusinessObjects.Mutable.Cache.Azure
{
    class UserIndexBlobClient
    {
        readonly SmartBlobClient blobClient;
        readonly string containerName;

        public UserIndexBlobClient(string storageAccountName, string storageKey, string containerName)
        {
            this.blobClient = new SmartBlobClient(storageAccountName, storageKey, false);
            this.containerName = containerName;
        }

        public async Task<BlobResult<UserIndex>> Get(Guid userId)
        {
            var blobName = userId.ToString("N");
            var result = await blobClient.Get<UserIndex>(containerName, blobName);
            return result;
        }

        public async Task<bool> Save(UserIndex user)
        {
            var requestProperties = new WriteRequestProperties
            {
                UseCompression = false,
            };
            var blobName = user.Id.ToString("N");
            try
            {
                await blobClient.Save(containerName, blobName, user, requestProperties);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
