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
        public async Task<HttpResponseMessage> GetImageCacheInfo(string url, int width, int height)
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
                using (var ms = new MemoryStream())
                {
                    resizedAndCropped.WriteToStream(ms, "image/jpeg", 92L);
                    ms.Position = 0;

                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(ms) };
                }
            }
        }
    }
}
