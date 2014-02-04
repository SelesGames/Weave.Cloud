using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ImageResizer.Role.Controllers
{
    public class ResizeController : ApiController
    {
        public async Task<HttpResponseMessage> Get(string url, int w, int h, string contentType = "image/jpeg", long imageQuality = 90L)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            var fill = new SolidBrush(Color.FromArgb(255, 255, 30, 30));

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var image = responseStream.ReadImage())
            using (var resized = image.Resize(w, h, fill, ImageExtensions.Stretch.UniformToFill))
            {
                var ms = new MemoryStream();
                resized.WriteToStream(ms, contentType, imageQuality);
                ms.Position = 0;

                var content = new StreamContent(ms);
                content.Headers.Add("Content-Type", contentType);

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
            }
        }
    }
}
