using SelesGames.Common.Hashing;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Weave.Mobilizer.Cache;
using Weave.Mobilizer.Contracts;
using Weave.Mobilizer.Core.Web;
using Weave.Mobilizer.DTOs;

namespace Weave.Mobilizer.Core.Controllers
{
    public class IPFController : ApiController, IMobilizerService
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

        public async Task<ReadabilityResult> Get(string url, bool stripLeadImage = false)
        {
            var result = await cache.Get(UrlToFileName(url));

            if (stripLeadImage)
            {
                //var sw = System.Diagnostics.Stopwatch.StartNew();
                var parser = new Weave.Mobilizer.HtmlParser.Parser();
                parser.RemoveImageFromContentMatchingLead(result);
                //sw.Stop();
                //System.Diagnostics.Debug.WriteLine(string.Format("elapsed time: {0} ms", sw.ElapsedMilliseconds));
            }
            return result;
        }

        [TokenAuthorizationAttribute]
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
