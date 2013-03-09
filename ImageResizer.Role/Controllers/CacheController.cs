using Common.Azure;
using System;
using System.Collections.Generic;
using System.Drawing;
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

                    // don't bother upscaling images
                    //if (width > originalImageWidth && height > originalImageHeight)
                    //    continue;


                    var blobFileName = string.Format("{0}-{1}.{2}", blobBaseFileName, outputSize.AppendString, settings.OutputFileExtension);

                    using (var ms = new MemoryStream())
                    {
                        using (var resizedAndCropped = image.CropAndResizeTo(width, height))
                        {
                            resizedAndCropped.WriteToStream(ms, contentType, settings.ImageQuality);
                            ms.Position = 0;
                        }

                        await blobClient.Save(blobFileName, ms);
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

        AzureBlobStreamClient CreateBlobClient(string contentType)
        {
            return new AzureBlobStreamClient(settings.AzureStorageAccountName, settings.AzureStorageKey, settings.BlobImageContainer, false)
            {
                ContentType = contentType
            };
        }
    }
}
