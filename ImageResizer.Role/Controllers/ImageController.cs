using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ImageResizer.Role.Controllers
{
    public class ImageController : ApiController
    {
        public async Task<HttpResponseMessage> GetImageCacheInfo(string url, int width, int height, string contentType = "image/jpeg")
        {
            int originalImageWidth;
            int originalImageHeight;

            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var image = responseStream.ReadImage())
            {
                originalImageWidth = image.Width;
                originalImageHeight = image.Height;

                using (var resizedAndCropped = image.CropAndResizeTo(width, height))
                { 
                    var ms = new MemoryStream();
                    resizedAndCropped.WriteToStream(ms, contentType, 92L);
                    ms.Position = 0;

                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", contentType);

                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
                }
            }
        }
    }
}
