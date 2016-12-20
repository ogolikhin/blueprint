using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Utilities
{
    public static class ImageUtilities
    {
        /// <summary>
        /// Generates a random image file.
        /// Copied from:  http://stackoverflow.com/questions/23781364/generating-a-random-jpg-image-from-console-application
        /// </summary>
        /// <param name="width">The image width.</param>
        /// <param name="height">The image height.</param>
        /// <param name="imageFormat">The image format (ex. Jpeg, Png...)</param>
        /// <returns>The raw bytes of the generated image.</returns>
        public static byte[] GenerateRandomImage(int width, int height, ImageFormat imageFormat)
        {
            // 1. Create a bitmap
            using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                // 2. Get access to the raw bitmap data
                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

                // 3. Generate RGB noise and write it to the bitmap's buffer.
                // Note that we are assuming that data.Stride == 3 * data.Width for simplicity/brevity here.
                byte[] noise = new byte[data.Width * data.Height * 3];
                new Random().NextBytes(noise);
                Marshal.Copy(noise, 0, data.Scan0, noise.Length);

                bitmap.UnlockBits(data);

                // 4. Save as the requested image format and convert to Base64.
                using (MemoryStream imageStream = new MemoryStream())
                {
                    bitmap.Save(imageStream, imageFormat);
                    return imageStream.ToArray();
                }
            }
        }
    }
}
