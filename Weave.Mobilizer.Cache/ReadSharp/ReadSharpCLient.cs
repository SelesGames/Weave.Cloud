using ReadSharp;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Weave.Services.Mobilizer.DTOs;

namespace Weave.Mobilizer.Cache.ReadSharp
{
    public class ReadSharpClient : IMobilizer
    {
        public async Task<MobilizerResult> Mobilize(string url)
        {
            try
            {
                var originalUrl = Uri.UnescapeDataString(url);
                var reader = new Reader();
                var readOptions = new ReadOptions
                {
                    HasHeadline = true,
                    PrettyPrint = false,
                    MultipageDownload = true,
                };
                var article = await reader.Read(new Uri(originalUrl)).ConfigureAwait(false);

                return new MobilizerResult
                {
                    title = article.Title,
                    author = null,
                    content = article.Content,
                    word_count = article.WordCount.ToString(),
                    lead_image_url = article.FrontImage == null ? null : article.FrontImage.OriginalString,
                    url = originalUrl,
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
