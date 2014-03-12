using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ImageResizer.Role.Controllers
{
    public class InfoController : ApiController
    {
        static Guid JPEG = ImageFormat.Jpeg.Guid;
        static Guid PNG = ImageFormat.Png.Guid;
        static Guid BMP = ImageFormat.Bmp.Guid;
        static Guid GIF = ImageFormat.Gif.Guid;

        string TryGetContentType(HttpResponseMessage response)
        {
            try
            {
                return response.Content.Headers.ContentType.MediaType;
            }
            catch{}
            return null;
        }

        public async Task<object> Get(string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var image = responseStream.ReadImage())
            {
                var imageWidth = image.Width;
                var imageHeight = image.Height;
                var contentLength = responseStream.Length;

                var contentType = TryGetContentType(response);
                string imageFormat = "unrecognized";

                if (image.RawFormat != null)
                {
                    var formatGuid = image.RawFormat.Guid;

                    if (formatGuid == JPEG)
                        imageFormat = "jpeg";

                    else if (formatGuid == PNG)
                        imageFormat = "png";

                    else if (formatGuid == BMP)
                        imageFormat = "bmp";

                    else if (formatGuid == GIF)
                        imageFormat = "gif";
                }

                return new
                {
                    ImageWidth = imageWidth,
                    ImageHeight = imageHeight,
                    ImageFormat = imageFormat,
                    ContentType = contentType,
                    ContentLength = contentLength,
                };
            }
        }
    }
}
