using SelesGames.Common.Hashing;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Weave.Mobilizer.Cache;
using Weave.Mobilizer.Contracts;
using Weave.Mobilizer.Core.Web;
using Weave.Mobilizer.DTOs;

namespace Weave.Mobilizer.WorkerRole.Controllers
{
    public class IPFController : ApiController, IMobilizerService
    {
        static readonly int MAX_AZURE_BLOB_NAME = 1024;
        static readonly int GUID_NO_HYPHENS_LENGTH = 32;

        MobilizerResultCache cache;
        AzureClient writeClient;

        public IPFController(MobilizerResultCache cache, AzureClient writeClient)
        {
            this.cache = cache;
            this.writeClient = writeClient;
        }

        public async Task<MobilizerResult> Get(string url, bool stripLeadImage = false)
        {
            try
            {
                NewRelic.Api.Agent.NewRelic.AddCustomParameter("URL", url);
            }
            catch { }
            var result = await cache.Get(UrlToFileName(url));
            return result;
        }

        [TokenAuthorizationAttribute]
        public Task Post(string url, [FromBody] MobilizerResult article)
        {
            return writeClient.Save(UrlToFileName(url), article);
        }




        #region Private helper methods

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

        #endregion
    }
}
