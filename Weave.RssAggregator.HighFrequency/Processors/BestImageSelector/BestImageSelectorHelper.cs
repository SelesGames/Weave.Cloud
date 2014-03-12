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
            if (bestImage == null)
                return;

            entry.OriginalImageUrl = bestImage.ImageUrl;
            entry.ImageWidth = bestImage.ImageWidth;
            entry.ImageHeight = bestImage.ImageHeight;
        }

        async Task<ImageInfo> SelectBestImage()
        {
            var original = TryGetImageInfoFromEntry();
            var mobilized = TryGetImageInfoFromMobilized();
            await Task.WhenAll(original, mobilized);

            var imageInfoOriginal = original.Result;
            var imageInfoMobilized = mobilized.Result;

            return SelectBestImage(imageInfoOriginal, imageInfoMobilized);
        }

        // We use the criteria of largest image (in bytes) as being the best image available
        ImageInfo SelectBestImage(params ImageInfo[] imageInfo)
        {
            return imageInfo.OfType<ImageInfo>().OrderByDescending(o => o.ContentLength).FirstOrDefault();
        }

        async Task<ImageInfo> TryGetImageInfoFromEntry()
        {
            ImageInfo imageInfo = null;

            var imageUrl = entry.OriginalImageUrl;

            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            try
            {
                imageInfo = await imageInfoClient.Get(imageUrl);
                imageInfo.ImageUrl = imageUrl;
            }
            catch { }
            return imageInfo;
        }

        async Task<ImageInfo> TryGetImageInfoFromMobilized()
        {
            ImageInfo imageInfo = null;

            try
            {
                var mobilizedResult = await GetMobilizedRepresentation();
                var imageUrl = mobilizedResult.lead_image_url;

                if (string.IsNullOrWhiteSpace(imageUrl))
                    return null;

                imageInfo = await imageInfoClient.Get(imageUrl);
                imageInfo.ImageUrl = imageUrl;
            }
            catch { }
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
