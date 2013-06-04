using System.Linq;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;

namespace Weave.RssAggregator.HighFrequency
{
    public class MobilizerOverride : ISequentialAsyncProcessor<HighFrequencyFeedUpdateDto>
    {
        readonly static string token = "hxyuiplkx78!ksdfl";
        readonly MobilizerServiceClient client;

        public MobilizerOverride()
        {
            client = new MobilizerServiceClient(token);
        }

        public bool IsHandledFully { get { return false; } }

        public async Task ProcessAsync(HighFrequencyFeedUpdateDto o)
        {
            if (o.RequiresMobilizerUpload())
            {
                await Task.WhenAll(o.Entries.Select(x => ProcessEntry(x, o)));
            }
        }

        async Task ProcessEntry(EntryWithPostProcessInfo e, HighFrequencyFeedUpdateDto feed)
        {
            try
            {
                await client.Post(e.Link,
                    new Weave.Mobilizer.DTOs.ReadabilityResult
                    {
                        title = e.Title,
                        url = e.Link,
                        date_published = e.UtcPublishDateTimeString,
                        domain = feed.FeedUri,
                        content = e.Description,
                        author = null,
                        word_count = "not available",
                    });
            }
            catch { }
        }
    }

    internal static class HighFrequencyFeedUpdateDtoExtensions
    {
        readonly static string MOBILIZER_UPLOAD = "mu";

        internal static bool RequiresMobilizerUpload(this HighFrequencyFeedUpdateDto o)
        {
            return o.Instructions == null ? false : o.Instructions.Contains(MOBILIZER_UPLOAD);
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
