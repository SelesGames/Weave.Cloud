using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace System.Drawing
{
    public class TestTest
    {
        public void Example()
        {
            using (var s = new MemoryStream())
            using (var originalImage = s.ReadImage())
            using (var resizedImage = originalImage.Resize(200, 400))
            using (var croppedImage = resizedImage.Crop(new Rectangle()))
            {
                croppedImage.WriteToStream(new MemoryStream(), "image/jpeg", 100);
            }
        }
    }

    public static class ImageExtensions
    {
        public static Image ReadImage(this Stream stream)
        {
            return Image.FromStream(stream);
        }

        public static Image Resize(this Image imgToResize, int targetWidth, int targetHeight)
        {
            Bitmap b = new Bitmap(targetWidth, targetHeight);
            using (Graphics g = Graphics.FromImage((Image)b))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(imgToResize, 0, 0, targetWidth, targetHeight);
            }
            return b;
        }

        public static Image Crop(this Image img, Rectangle cropArea)
        {
            Bitmap bmpImage;

            if (img is Bitmap)
            {
                bmpImage = (Bitmap)img;
                return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            }

            else
            {
                using (bmpImage = new Bitmap(img))
                {
                    return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
                }
            }
        }

        public enum MergeType
        {
            Horizontal,
            Vertical
        }

        public static Image Merge(this IList<Image> images, MergeType mergeType)
        {
            int targetWidth, targetHeight;

            if (mergeType == MergeType.Horizontal)
            {
                targetWidth = images.Sum(o => o.Width);
                targetHeight = images.Max(o => o.Height);
            }
            else if (mergeType == MergeType.Vertical)
            {
                targetWidth = images.Max(o => o.Width);
                targetHeight = images.Sum(o => o.Height);
            }
            else
                throw new Exception("mergetype all wrong dude");

            Bitmap b = new Bitmap(targetWidth, targetHeight);
            using (Graphics g = Graphics.FromImage((Image)b))
            {
                //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                int startX = 0;
                int startY = 0;
                foreach (var image in images)
                {
                    var width = image.Width;
                    var height = image.Height;
                    g.DrawImage(image, startX, startY, width, height);
                    if (mergeType == MergeType.Horizontal)
                        startX += width;
                    else if (mergeType == MergeType.Vertical)
                        startY += height;
                }
            }
            return b;
        }

        public static void WriteToStream(this Image img, Stream stream, string mimeType, long quality)
        {
            // image codec
            ImageCodecInfo codecInfo = mimeType.GetEncoderInfo();

            if (codecInfo == null)
                throw new Exception(string.Format("invalid mimeType - {0} is not a valid codec", mimeType));

            // Encoder parameter for image quality
            EncoderParameter qualityParam =
                new EncoderParameter(Encoder.Quality, quality);

            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            img.Save(stream, codecInfo, encoderParams);
        }

        static ImageCodecInfo GetEncoderInfo(this string mimeType)
        {
            // Get image codecs for all image formats
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            return null;
        }
    }


}

//namespace SelesGames.Drawing
//{
//    public class ImageResizer
//    {
//        private Image img;

//        void OpenImage(Stream stream)
//        {
//            this.img = Image.FromStream(stream);
//        }

//        private Image cropImage(Image img, Rectangle cropArea)
//        {
//            Bitmap bmpImage = new Bitmap(img);
//            Bitmap bmpCrop = bmpImage.Clone(cropArea,
//                                            bmpImage.PixelFormat);
//            return (Image)(bmpCrop);
//        }

//        private Image resizeImage(Image imgToResize, Size size)
//        {
//            int sourceWidth = imgToResize.Width;
//            int sourceHeight = imgToResize.Height;

//            float nPercent = 0;
//            float nPercentW = 0;
//            float nPercentH = 0;

//            nPercentW = ((float)size.Width / (float)sourceWidth);
//            nPercentH = ((float)size.Height / (float)sourceHeight);

//            if (nPercentH < nPercentW)
//                nPercent = nPercentH;
//            else
//                nPercent = nPercentW;

//            int destWidth = (int)(sourceWidth * nPercent);
//            int destHeight = (int)(sourceHeight * nPercent);

//            Bitmap b = new Bitmap(destWidth, destHeight);
//            Graphics g = Graphics.FromImage((Image)b);
//            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

//            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
//            g.Dispose();

//            return (Image)b;
//        }

//        private void saveJpeg(string path, Bitmap img, long quality)
//        {
//            // Encoder parameter for image quality
//            EncoderParameter qualityParam =
//                new EncoderParameter(Encoder.Quality, quality);

//            // Jpeg image codec
//            ImageCodecInfo jpegCodec = getEncoderInfo("image/jpeg");

//            if (jpegCodec == null)
//                return;

//            EncoderParameters encoderParams = new EncoderParameters(1);
//            encoderParams.Param[0] = qualityParam;

//            img.Save(path, jpegCodec, encoderParams);
//        }


//        private ImageCodecInfo getEncoderInfo(string mimeType)
//        {
//            // Get image codecs for all image formats
//            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

//            // Find the correct image codec
//            for (int i = 0; i < codecs.Length; i++)
//                if (codecs[i].MimeType == mimeType)
//                    return codecs[i];
//            return null;
//        }
//    }
//}
