using ReadSharp;
using SelesGames.HttpClient;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Weave.Mobilizer.DTOs;

namespace Weave.Readability
{
//    public class ReadabilityClient
//    {
//        readonly string urlTemplate = "https://www.readability.com/api/content/v1/parser?token={0}&url={1}";
//        readonly string token;

//        public ReadabilityClient(string token)
//        {
//            this.token = token;
//        }

//        public Task<MobilizerResult> GetAsync(string url)
//        {
//            var fullUrl = string.Format(urlTemplate, token, url);
//#if DEBUG
//            System.Diagnostics.Debug.WriteLine(string.Format("calling {0}", url), "READABILITY");
//#endif
//            return new SmartHttpClient(ContentEncoderSettings.Json, CompressionSettings.OnContent | CompressionSettings.OnRequest)
//                .GetAsync<MobilizerResult>(fullUrl, CancellationToken.None);
//        }
//    }

    public class ReadabilityClient
    {
        public async Task<MobilizerResult> GetAsync(string url)
        {
            try
            {
                var originalUrl = Uri.UnescapeDataString(url);
                var reader = new Reader();
                var article = await reader.Read(new Uri(originalUrl)).ConfigureAwait(false);


                return new MobilizerResult
                {
                    title = article.Title,
                    author = null,
                    content = article.Content,
                    word_count = article.WordCount.ToString(),
                    lead_image_url = article.FrontImage == null ? null : article.FrontImage.OriginalString,
                    url = url,
                    date_published = null,
                    domain = null,
                };
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                throw;
            }
        }
    }
}
