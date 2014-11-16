using HtmlAgilityPack;
using SelesGames.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weave.Mobilizer.Client;
using Weave.Updater.BusinessObjects;

namespace Weave.FeedUpdater.HighFrequency.Processors.BestImageSelector
{
    class BestImageSelectorHelper
    {
        readonly static TimeSpan IMAGE_ACQ_TIMEOUT = TimeSpan.FromSeconds(7);

        readonly Weave.Services.Mobilizer.Client mobilizerClient;
        readonly ExpandedEntry entry;
        readonly ImageInfoClient imageInfoClient;
        readonly List<ImageInfo> imagePool;

        public BestImageSelectorHelper(ExpandedEntry entry)
        {
            this.entry = entry;
            this.mobilizerClient = new Weave.Services.Mobilizer.Client();
            this.imageInfoClient = new ImageInfoClient { Timeout = IMAGE_ACQ_TIMEOUT };
            this.imagePool = new List<ImageInfo>();
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
        async Task<Option<ImageInfo>> GetFromUrl(string url)
        {
            try
            {
                var imageInfo = await imageInfoClient.Get(url);
                imageInfo.ImageUrl = url;
                return Option.Some(imageInfo);
            }
            catch (InvalidImageException)
            {
                return Option.None<ImageInfo>();
            }
            catch
            {
                var imageInfo = new ImageInfo { ImageUrl = url };
                return Option.Some(imageInfo);
            }
        }

        static IEnumerable<ImageInfo> Filter(IEnumerable<Option<ImageInfo>> images)
        {
            //return images.OfType<ImageInfo>().Where(o =>
            return images.SelectMany(o => o).Where(o =>
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
                var mobilized = await mobilizerClient.Get(url, stripLeadImage: false);

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
                node != null && 
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