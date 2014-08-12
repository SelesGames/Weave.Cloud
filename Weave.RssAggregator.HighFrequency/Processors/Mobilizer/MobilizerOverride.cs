using System.Linq;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;
using Weave.Mobilizer.DTOs;
using Weave.Mobilizer.HtmlParser;
using Weave.Updater.BusinessObjects;

namespace Weave.RssAggregator.HighFrequency
{
    public class MobilizerOverride : ISequentialAsyncProcessor<FeedUpdate>
    {
        readonly static string token = "hxyuiplkx78!ksdfl";
        readonly MobilizerServiceClient client;

        public MobilizerOverride()
        {
            client = new MobilizerServiceClient(token);
        }

        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(FeedUpdate o)
        {
            if (o.RequiresMobilizerUpload())
            {
                await Task.WhenAll(o.Entries.Select(x => ProcessEntry(x, o)));
            }
        }

        async Task ProcessEntry(ExpandedEntry e, FeedUpdate feed)
        {
            try
            {
                var bestImage = e.Images.GetBest();

                var readabilityResult = new MobilizerResult
                {
                    title = e.Title,
                    url = e.Link,
                    date_published = e.UtcPublishDateTimeString,
                    domain = feed.Feed.Uri,
                    content = e.Description,
                    author = null,
                    word_count = "not available",
                    lead_image_url = bestImage == null ? null : bestImage.Url,
                };

                var parser = new Parser();
                parser.ProcessContent(readabilityResult);

                await client.Post(e.Link, readabilityResult);
            }
            catch { }
        }
    }

    internal static class FeedUpdateExtensions
    {
        readonly static string MOBILIZER_UPLOAD = "mu";

        internal static bool RequiresMobilizerUpload(this FeedUpdate o)
        {
            return o.Feed.Instructions == null ? false : o.Feed.Instructions.Contains(MOBILIZER_UPLOAD);
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
