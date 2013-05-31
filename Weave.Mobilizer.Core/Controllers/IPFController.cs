using SelesGames.Common.Hashing;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Weave.Mobilizer.Cache;
using Weave.Readability;

namespace Weave.Mobilizer.Core.Controllers
{
    public class IPFController : ApiController
    {
        static readonly int MAX_AZURE_BLOB_NAME = 1024;
        static readonly int GUID_NO_HYPHENS_LENGTH = 32;

        ReadabilityCache cache;
        AzureClient writeClient;

        public IPFController(ReadabilityCache cache, AzureClient writeClient)
        {
            this.cache = cache;
            this.writeClient = writeClient;
        }

        public Task<ReadabilityResult> Get(string url)
        {
            return cache.Get(UrlToFileName(url));
        }

        public Task Post(string url, [FromBody] ReadabilityResult article)
        {
            return writeClient.Save(UrlToFileName(url), article);
        }

        // approach using MD5 and GUIDs
        string UrlToFileName(string url)
        {
            string fileName = null;

            var encoded = HttpUtility.UrlEncode(url);
            bool isFileNameTooLong = encoded.Length > MAX_AZURE_BLOB_NAME;

            if (isFileNameTooLong)
            {
                var guid = CryptoHelper.ComputeHashUsedByMobilizer(encoded);
                var guidChars = guid.ToString("N");
                fileName = encoded.Substring(0, MAX_AZURE_BLOB_NAME - GUID_NO_HYPHENS_LENGTH) + guidChars;
            }
            else
            {
                fileName = encoded;
            }

            return fileName;
        }
    }
}
