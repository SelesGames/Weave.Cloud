using Common.Azure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
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

            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (response.StatusCode != HttpStatusCode.OK)
                return null;


            var savedFileNames = new List<string>();

            var contentType = settings.OutputContentType;

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var image = responseStream.ReadImage())
            {
                originalImageWidth = image.Width;
                originalImageHeight = image.Height;

                string blobBaseFileName = Guid.NewGuid().ToString();
                var blobClient = CreateBlobClient(contentType);


                foreach (var outputSize in settings.OutputSizes)
                {
                    var width = outputSize.Size.Width;
                    var height = outputSize.Size.Height;

                    using (var resizedAndCropped = image.CropAndResizeTo(width, height))
                    using (var ms = new MemoryStream())
                    {
                        resizedAndCropped.WriteToStream(ms, contentType, 92L);
                        ms.Position = 0;

                        var blobFileName = string.Format("{0}-{1}.{2}", blobBaseFileName, outputSize.AppendString, settings.OutputFileExtension);
                        await blobClient.Save(blobFileName, ms);


                        var fullFilePath = string.Format("{0}{1}/{2}", blobClient.BlobEndpoint, settings.BlobImageContainer, blobFileName);
                        savedFileNames.Add(fullFilePath);
                    }                
                }
            }

            return savedFileNames;
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
