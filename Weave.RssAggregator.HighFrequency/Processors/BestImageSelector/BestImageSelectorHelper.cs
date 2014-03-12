using System.Linq;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;
using Weave.Mobilizer.DTOs;

namespace Weave.RssAggregator.HighFrequency.Processors.BestImageSelector
{
    class BestImageSelectorHelper
    {
        readonly static string token = "hxyuiplkx78!ksdfl";
        readonly MobilizerServiceClient mobilizerClient;
        readonly EntryWithPostProcessInfo entry;
        readonly ImageInfoClient imageInfoClient;

        public BestImageSelectorHelper(EntryWithPostProcessInfo entry)
        {
            this.entry = entry;
            mobilizerClient = new MobilizerServiceClient(token);
            imageInfoClient = new ImageInfoClient();
        }

        public async Task Process()
        {
            var bestImage = await SelectBestImage();
            Apply(bestImage);
        }

        void Apply(ImageInfo bestImage)
        {
            entry.OriginalImageUrl = bestImage.ImageUrl;
            entry.ImageWidth = bestImage.ImageWidth;
            entry.ImageHeight = bestImage.ImageHeight;
        }

        async Task<ImageInfo> SelectBestImage()
        {
            var original = GetImageInfoFromEntry();
            var mobilized = GetImageInfoFromMobilized();
            await Task.WhenAll(original, mobilized);

            var imageInfoOriginal = original.Result;
            var imageInfoMobilized = mobilized.Result;

            return SelectBestImage(imageInfoOriginal, imageInfoMobilized);
        }

        // We use the criteria of largest image (in bytes) as being the best image available
        ImageInfo SelectBestImage(params ImageInfo[] imageInfo)
        {
            return imageInfo.OrderByDescending(o => o.ContentLength).First();
        }

        async Task<ImageInfo> GetImageInfoFromEntry()
        {
            var imageUrl = entry.OriginalImageUrl;
            var imageInfo = await imageInfoClient.Get(imageUrl);
            imageInfo.ImageUrl = imageUrl;
            return imageInfo;
        }

        async Task<ImageInfo> GetImageInfoFromMobilized()
        {
            var mobilizedResult = await GetMobilizedRepresentation();
            var imageUrl = mobilizedResult.lead_image_url;
            var imageInfo = await imageInfoClient.Get(imageUrl);
            imageInfo.ImageUrl = imageUrl;
            return imageInfo;
        }

        async Task<MobilizerResult> GetMobilizedRepresentation()
        {
            var url = entry.Link;
            var mobilized = await mobilizerClient.Get(url);
            return mobilized;
        }
    }
}
