using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;

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
            var images = await GetImages();
            foreach (var image in images)
                entry.Images.Add(image);

            var bestImage = SelectBestImage(images);
            if (bestImage != null)
            {
                entry.Image.ShouldIncludeImage = true;
                entry.Image.OriginalUrl = bestImage.Url;
                entry.Image.PreferredUrl = bestImage.Url;
                entry.Image.Width = bestImage.Width;
                entry.Image.Height = bestImage.Height;
            }
        }

        async Task<IEnumerable<Image>> GetImages()
        {
            var imageTasks = entry.ImageUrls
                .Select(GetFromUrl)
                .Union(new Task<ImageInfo>[]{ GetFromMobilizedRep() });

            var imagesWithInfo = await Task.WhenAll(imageTasks);

            var images = imagesWithInfo.OfType<ImageInfo>().Select(Map).ToList();
            return images;
        }

        /// <summary>
        /// Call the image info weave service to get image info (height, width, etc.)
        /// </summary>
        async Task<ImageInfo> GetFromUrl(string url)
        {
            ImageInfo imageInfo = null;

            try
            {
                imageInfo = await imageInfoClient.Get(url);
                imageInfo.ImageUrl = url;
            }
            catch(InvalidImageException)
            {
                return null;
            }
            catch 
            {
                imageInfo = new ImageInfo { ImageUrl = url };
            }
            return imageInfo;
        }

        async Task<ImageInfo> GetFromMobilizedRep()
        {
            ImageInfo imageInfo = null;

            try
            {
                var url = entry.Link;
                var mobilized = await mobilizerClient.Get(url);
                var imageUrl = mobilized.lead_image_url;

                if (string.IsNullOrWhiteSpace(imageUrl))
                    return null;

                imageInfo = await imageInfoClient.Get(imageUrl);
                imageInfo.ImageUrl = imageUrl;
            }
            catch { }
            return imageInfo;
        }

        static Image Map(ImageInfo o)
        {
            return new Image
            {
                Url = o.ImageUrl,
                Width = o.ImageWidth,
                Height = o.ImageHeight,
                ContentLength = o.ContentLength,
                ContentType = o.ContentType,
            };
        }

        // We use the criteria of largest image (in bytes) as being the best image available
        static Image SelectBestImage(IEnumerable<Image> images)
        {
            return images.OrderByDescending(o => o.ContentLength).FirstOrDefault();
        }
    }
}