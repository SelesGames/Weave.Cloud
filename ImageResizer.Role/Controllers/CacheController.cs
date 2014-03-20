using Common.Azure.Blob;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ImageResizer.Role.Controllers
{
    public class CacheController : ApiController
    {
        static Guid jpegGuid = ImageFormat.Jpeg.Guid;

        Settings settings;

        public CacheController(Settings settings)
        {
            this.settings = settings;
        }

        public async Task<object> Get(string url)
        {
            int originalImageWidth;
            int originalImageHeight;
            string fullFilePath;
            string baseImageUrl;

            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (response.StatusCode != HttpStatusCode.OK)
                return null;


            var savedFileNames = new List<string>();
            var supportedFormats = new StringBuilder();


            var contentType = settings.OutputContentType;

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var image = responseStream.ReadImage())
            {
                originalImageWidth = image.Width;
                originalImageHeight = image.Height;

                string blobBaseFileName = Guid.NewGuid().ToString();
                var blobClient = CreateBlobClient(contentType);


                baseImageUrl = string.Format("{0}{1}/{2}", blobClient.BlobEndpoint, settings.BlobImageContainer, blobBaseFileName);

                foreach (var outputSize in settings.OutputSizes)
                {
                    var width = outputSize.Size.Width;
                    var height = outputSize.Size.Height;

                    if (image.RawFormat != null && image.RawFormat.Guid == jpegGuid)
                    {
                        var originalArea = originalImageWidth * originalImageHeight;
                        var requestedArea = width * height;
                        // don't bother upscaling images
                        if ((1.618 * requestedArea) > originalArea)
                            continue;
                    }


                    var blobFileName = string.Format("{0}-{1}.{2}", blobBaseFileName, outputSize.AppendString, settings.OutputFileExtension);

                    using (var ms = new MemoryStream())
                    {
                        using (var resizedAndCropped = image.Resize(width, height, Stretch.UniformToFill))
                        {
                            resizedAndCropped.WriteToStream(ms, contentType, settings.ImageQuality);
                            ms.Position = 0;
                        }

                        await blobClient.SaveBlobContent(settings.BlobImageContainer, blobFileName, ms);
                    }

                    fullFilePath = string.Format("{0}{1}/{2}", blobClient.BlobEndpoint, settings.BlobImageContainer, blobFileName);
                    savedFileNames.Add(fullFilePath);
                    supportedFormats.Append(string.Format("{0},", outputSize.AppendString));
                }

                if (supportedFormats.Length >= 1)
                    supportedFormats.Remove(supportedFormats.Length - 1, 1);
            }

            return new 
            { 
                ImageWidth = originalImageWidth, 
                ImageHeight = originalImageHeight,
                BaseImageUrl = baseImageUrl,
                SupportedFormats = supportedFormats.ToString(),
                FileExtension = settings.OutputFileExtension,
                SavedFileNames = savedFileNames,
            };
        }

        AzureBlobClient CreateBlobClient(string contentType)
        {
            return new AzureBlobClient(
                storageAccountName: settings.AzureStorageAccountName,
                key: settings.AzureStorageKey,
                useHttps: false);
            //return new AzureBlobClient(settings.AzureStorageAccountName, settings.AzureStorageKey, settings.BlobImageContainer, false)
            //{
            //    ContentType = contentType
            //};
        }
    }
}
