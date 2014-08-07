using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Weave.RssAggregator.HighFrequency
{
    public class ImageScalerUpdater : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        readonly string urlFormat = "http://weave-imagecache.cloudapp.net/api/cache?url={0}";
        HttpClient client;

        public ImageScalerUpdater()
        {
            IsHandledFully = false;
            client = new HttpClient() 
            { 
                Timeout = TimeSpan.FromMinutes(2),
            };
        }

        public bool IsHandledFully { get; private set; }

        public Task ProcessAsync(HighFrequencyFeedUpdateDto o)
        {
            return Task.WhenAll(o.Entries.Select(ProcessEntry)); 
        }

        async Task ProcessEntry(EntryWithPostProcessInfo e)
        {
            try
            {
                var imageUrl = e.Image.OriginalUrl;
                e.Image.PreferredUrl = imageUrl;

                if (string.IsNullOrWhiteSpace(imageUrl))
                    return;

                var url = string.Format(urlFormat, HttpUtility.UrlEncode(imageUrl));
                var response = await client.GetStringAsync(url);
                if (string.IsNullOrWhiteSpace(response))
                    return;

                var result = JsonConvert.DeserializeObject<ImageServiceResult>(response);
                if (result == null)
                    return;

                e.Image.Width = result.ImageWidth;
                e.Image.Height = result.ImageHeight;
                e.Image.BaseResizedUrl = result.BaseImageUrl;
                e.Image.SupportedFormats = result.SupportedFormats;
                
                if (!string.IsNullOrWhiteSpace(result.BaseImageUrl) &&
                    (result.ImageWidth * result.ImageHeight > 100) &&
                    result.SupportedFormatsList != null && 
                    result.SupportedFormatsList.Any(o => "sd".Equals(o, StringComparison.OrdinalIgnoreCase)))
                {
                    var resizeUrl = string.Format("{0}-sd.jpg", result.BaseImageUrl);
                    e.Image.PreferredUrl = resizeUrl;
                }

                e.Image.ShouldIncludeImage = true;
            }
            catch { }
        }
    }
}




//if (e.ResizedImagesUrls == null || !e.ResizedImagesUrls.Any())
//    return;

//var sd = e.ResizedImagesUrls.FirstOrDefault(o => o.EndsWith("-sd.jpg"));
//if (sd != null)
//    e.ImageUrl = e.ResizedImagesUrls.First();


        //string EvaluateForAspectRatio(imageServiceResult result)
        //{
        //    var width = result.ImageWidth;
        //    var height = result.ImageHeight;

        //    var aspect = (double)width / (double)height;

        //    // a 5:4 ratio is the cutoff for landscape
        //    if (aspect > 1.25)
        //        return "landscape";

        //    // a 4:5 ratio is cutoff for portrait
        //    if (aspect < 0.8)
        //        return "portrait";

        //    return "square";
        //}

        //bool EvaluateForImageQuality(imageServiceResult result)
        //{
        //    var width = result.ImageWidth;
        //    var height = result.ImageHeight;

        //    return (width * height) > 120000;
        //}
