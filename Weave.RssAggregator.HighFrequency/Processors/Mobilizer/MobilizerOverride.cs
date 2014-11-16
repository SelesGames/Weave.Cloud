using System.Linq;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;
using Weave.Mobilizer.HtmlParser;
using Weave.Services.Mobilizer.DTOs;
using Weave.Updater.BusinessObjects;

namespace Weave.FeedUpdater.HighFrequency
{
    public class MobilizerOverride : IAsyncProcessor<HighFrequencyFeedUpdate>
    {
        readonly static string token = "hxyuiplkx78!ksdfl";
        readonly MobilizerServiceClient client;

        public MobilizerOverride()
        {
            client = new MobilizerServiceClient();
        }

        public async Task ProcessAsync(HighFrequencyFeedUpdate o)
        {
            if (o.RequiresMobilizerUpload())
            {
                await Task.WhenAll(o.Entries.Select(x => ProcessEntry(x, o)));
            }
        }

        async Task ProcessEntry(ExpandedEntry e, HighFrequencyFeedUpdate feed)
        {
            if (e.Description == null)
                return;

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
    }

    internal static class HighFrequencyFeedUpdateExtensions
    {
        readonly static string MOBILIZER_UPLOAD = "mu";

        internal static bool RequiresMobilizerUpload(this HighFrequencyFeedUpdate o)
        {
            return o.Feed.Instructions == null ? false : o.Feed.Instructions.Contains(MOBILIZER_UPLOAD);
        }
    }
}