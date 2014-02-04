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
        static Brush TRANSPARENT_BRUSH = new SolidBrush(Color.Transparent);




        #region Resize operation, with 4 sizing modes

        public static Image Resize(this Image imgToResize,
            int targetWidth,
            int targetHeight,
            Stretch stretch = Stretch.Fill)
        {
            return Resize(imgToResize, targetWidth, targetHeight, TRANSPARENT_BRUSH, stretch);
        }

        public static Image Resize(this Image imgToResize, 
            int targetWidth, 
            int targetHeight, 
            Brush fill,
            Stretch stretch = Stretch.Fill)
        {
            Bitmap b = new Bitmap(targetWidth, targetHeight);

            // set the resolutions the same to avoid cropping due to resolution differences
            b.SetResolution(imgToResize.HorizontalResolution, imgToResize.VerticalResolution);
            
            using (Graphics g = Graphics.FromImage((Image)b))
            {
                g.FillRectangle(fill, 0, 0, targetWidth, targetHeight);

                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingMode = CompositingMode.SourceOver;
                //var imageAttributes = new ImageAttributes();
                //imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

                var targets = new ResizeTargets();

                if (stretch == Stretch.Fill)
                    targets = new ResizeTargets { Width = targetWidth, Height = targetHeight, XOffset = 0, YOffset = 0 };
            
                else if (stretch == Stretch.Uniform)
                    targets = GetResizeForUniform(imgToResize.Width, imgToResize.Height, targetWidth, targetHeight);

                else if (stretch == Stretch.UniformToFill)
                    targets = GetResizeForUniformToFill(imgToResize.Width, imgToResize.Height, targetWidth, targetHeight);

                else if (stretch == Stretch.None)
                    targets = GetResizeForNone(imgToResize.Width, imgToResize.Height, targetWidth, targetHeight);

                g.DrawImage(imgToResize, targets.XOffset, targets.YOffset, targets.Width, targets.Height);
            }
            return b;
        }

        #endregion




        #region Crop operation

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

        #endregion




        #region Merge operation - combine multiple images into one image

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

        #endregion




        #region Stream/Image operations

        public static Image ReadImage(this Stream stream)
        {
            //return new Bitmap(stream, false);
            return Image.FromStream(stream);
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

        #endregion




        #region private helper methods

        struct ResizeTargets
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int XOffset { get; set; }
            public int YOffset { get; set; }
        }

        static ResizeTargets GetResizeForUniform(int width, int height, int targetWidth, int targetHeight)
        {
            var aspectRatio = (double)width / (double)height;
            var targetAspectRatio = (double)targetWidth / (double)targetHeight;

            double scale;
            int resizeWidth, resizeHeight, xOffset, yOffset;

            // original aspect ratio is taller than the target aspect ratio
            if (aspectRatio < targetAspectRatio)
            {
                scale = (double)targetHeight / (double)height;
                resizeWidth = (int)(width * scale);
                resizeHeight = targetHeight;
                xOffset = (int)((double)(targetWidth - resizeWidth) / 2d);
                yOffset = 0;
            }

            // original aspect ratio is wider than the target aspect ratio
            else
            {
                scale = (double)targetWidth / (double)width;
                resizeWidth = targetWidth;
                resizeHeight = (int)(height * scale);
                xOffset = 0;
                yOffset = (int)((double)(targetHeight - resizeHeight) / 2d);
            }

            return new ResizeTargets
            {
                Width = resizeWidth,
                Height = resizeHeight,
                XOffset = xOffset,
                YOffset = yOffset,
            };
        }

        static ResizeTargets GetResizeForUniformToFill(int width, int height, int targetWidth, int targetHeight)
        {
            var aspectRatio = (double)width / (double)height;
            var targetAspectRatio = (double)targetWidth / (double)targetHeight;

            double scale;
            int resizeWidth, resizeHeight, xOffset, yOffset;

            // original aspect ratio is taller than the target aspect ratio
            if (aspectRatio < targetAspectRatio)
            {
                scale = (double)targetWidth / (double)width;
                resizeWidth = targetWidth;
                resizeHeight = (int)(height * scale);
                xOffset = 0;
                yOffset = (int)((double)(targetHeight - resizeHeight) / 2d);
            }

            // original aspect ratio is wider than the target aspect ratio
            else
            {
                scale = (double)targetHeight / (double)height;
                resizeWidth = (int)(width * scale);
                resizeHeight = targetHeight;
                xOffset = (int)((double)(targetWidth - resizeWidth) / 2d);
                yOffset = 0;
            }

            return new ResizeTargets
            {
                Width = resizeWidth,
                Height = resizeHeight,
                XOffset = xOffset,
                YOffset = yOffset,
            };
        }

        static ResizeTargets GetResizeForNone(int width, int height, int targetWidth, int targetHeight)
        {
            int resizeWidth, resizeHeight, xOffset, yOffset;

            resizeWidth = width;
            resizeHeight = height;
            xOffset = (int)((double)(targetWidth - resizeWidth) / 2d);
            yOffset = (int)((double)(targetHeight - resizeHeight) / 2d);

            return new ResizeTargets
            {
                Width = resizeWidth,
                Height = resizeHeight,
                XOffset = xOffset,
                YOffset = yOffset,
            };
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

        #endregion
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
