﻿using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ImageResizer.Role.Controllers
{
    public class ImageController : ApiController
    {
        public async Task<HttpResponseMessage> GetImageCacheInfo(string url, int width, int height, string contentType = "image/jpeg", long imageQuality = 90L)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var image = responseStream.ReadImage())
            using (var resizedAndCropped = image.Resize(width, height, Stretch.UniformToFill))
            { 
                var ms = new MemoryStream();
                resizedAndCropped.WriteToStream(ms, contentType, imageQuality);
                ms.Position = 0;

                var content = new StreamContent(ms);
                content.Headers.Add("Content-Type", contentType);

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
            }
        }
    }
}
