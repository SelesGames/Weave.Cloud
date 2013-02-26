using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;

namespace ZuneCrawler.WcfService.Services.BackgroundImage
{
    public class ImageResizer
    {
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        public ImageResizer()
        {
            ImageWidth = 480;
            ImageHeight = 177;
        }

        public MemoryStream GetResizedImageFromWebResponseStream(WebResponse response)
        {
            if (response == null)
                throw new Exception("response in GetResizedImageFromWebResponseStream was null");


            using (var s = response.GetResponseStream())
            using (var originalImage = s.ReadImage())
            {
                var scale = (double)ImageWidth / (double)originalImage.Width;
                var targetHeight = scale * originalImage.Height;

                var yOffset = Math.Max(0, (targetHeight / 3) - (0.5d * ImageHeight));

                //using (var resizedImage = originalImage.Resize(480, 384))
                using (var resizedImage = originalImage.Resize((int)ImageWidth, (int)targetHeight))
                //using (var croppedImage = resizedImage.Crop(new Rectangle(0, 103, 480, 177))) // centered around the halfway point
                using (var croppedImage = resizedImage.Crop(new Rectangle(0, (int)yOffset, ImageWidth, ImageHeight))) // centered around the top 1/3 point
                //using (var croppedImage = resizedImage.Crop(new Rectangle(0, 39, 480, 177))) // centered around the top 1/3 point
                {
                    var ms = new MemoryStream();
                    croppedImage.WriteToStream(ms, "image/jpeg", 92L);
                    ms.Position = 0;
                    return ms;
                }
            }
        }
    }
}
