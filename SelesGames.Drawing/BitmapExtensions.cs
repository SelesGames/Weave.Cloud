using System.Drawing.Imaging;

namespace System.Drawing
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// Converts a Bitmap's pixels to a 1-dimensional array
        /// </summary>
        /// <param name="bmp">The Bitmap to extract the pixel data from</param>
        /// <returns>A byte array of the Bitmap's pixel data</returns>
        public static byte[] To1DByteArray(this Bitmap bmp)
        {
            if (bmp == null) throw new NullReferenceException("Bitmap is null");

            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = data.Scan0;

            //declare an array to hold the bytes of the bitmap
            int numBytes = data.Stride * bmp.Height;
            byte[] bytes = new byte[numBytes];

            //copy the RGB values into the array
            System.Runtime.InteropServices.Marshal.Copy(ptr, bytes, 0, numBytes);

            bmp.UnlockBits(data);

            return bytes;
        }
    }
}
