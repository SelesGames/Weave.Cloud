using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;
using Weave.Mobilizer.DTOs;
using Weave.Updater.BusinessObjects;

namespace Weave.RssAggregator.HighFrequency.Processors.BestImageSelector
{
    class BestImageSelectorHelper
    {
        readonly static string token = "hxyuiplkx78!ksdfl";
        readonly MobilizerServiceClient mobilizerClient;
        readonly ExpandedEntry entry;
        readonly ImageInfoClient imageInfoClient;
        readonly List<ImageInfo> imagePool;

        public BestImageSelectorHelper(ExpandedEntry entry)
        {
            this.entry = entry;
            mobilizerClient = new MobilizerServiceClient(token);
            imageInfoClient = new ImageInfoClient();
            imagePool = new List<ImageInfo>();
        }

        public async Task Process()
        {
            await LoadBestImages();
            var images = imagePool.Select(Map);
            foreach (var image in images)
                entry.Images.Add(image);
        }

        async Task LoadBestImages()
        {
            IEnumerable<string> urls = entry.ImageUrls;

            await AddImagesFromUrls(urls);
            if (imagePool.Any())
                return;

            urls = await GetImageUrlsFromMobilizer();
            await AddImagesFromUrls(urls);
        }




        #region Get ImageInfo metadata for a list of image urls

        async Task AddImagesFromUrls(IEnumerable<string> urls)
        {
            var images = await Task.WhenAll(urls.Select(GetFromUrl));
            foreach (var image in Filter(images))
            {
                imagePool.Add(image);
            }
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
            catch (InvalidImageException)
            {
                return null;
            }
            catch
            {
                imageInfo = new ImageInfo { ImageUrl = url };
            }
            return imageInfo;
        }

        static IEnumerable<ImageInfo> Filter(IEnumerable<ImageInfo> images)
        {
            return images.OfType<ImageInfo>().Where(o =>
            {
                var imageArea = o.ImageHeight * o.ImageWidth;
                return imageArea == 0 || imageArea > 10000;
            });
        }

        #endregion




        #region Get the mobilized rep of the article, extract image urls from there

        async Task<IEnumerable<string>> GetImageUrlsFromMobilizer()
        {
            try
            {
                var url = entry.Link;
                var mobilized = await mobilizerClient.Get(url);

                var html = mobilized.content;
                var doc = new HtmlDocument();
                doc.OptionDefaultStreamEncoding = Encoding.UTF8;
                doc.LoadHtml(html);
                return doc.DocumentNode
                    .Descendants()
                    .Where(IsImageNode)
                    .Select(o => o.Attributes["src"].Value)
                    .Union(new[] { mobilized.lead_image_url })
                    .OfType<string>()
                    .Distinct();
            }
            catch { return new List<string>(); }         
        }

        static bool IsImageNode(HtmlNode node)
        {
            return
                node.Name == "img" &&
                node.Attributes["src"] != null;// &&
                //mobilizerDetectedFirstImage.Equals(node.Attributes["src"].Value, StringComparison.OrdinalIgnoreCase);
        }

        #endregion




        #region Map function

        static Image Map(ImageInfo o)
        {
            return new Image
            {
                Url = o.ImageUrl,
                Width = o.ImageWidth,
                Height = o.ImageHeight,
                ContentLength = o.ContentLength,
                ContentType = o.ContentType,
                Format = o.ImageFormat,
            };
        }

        #endregion
    }
}