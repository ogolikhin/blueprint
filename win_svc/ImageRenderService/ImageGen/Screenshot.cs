using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ImageRenderService.ImageGen
{
    public class Screenshot : IScreenshot
    {
        public Screenshot(Bitmap image)
        {
            if (image == null)
            {
                throw new ArgumentNullException();
            }
            Image = image;
        }

        public void Save(Stream stream, ImageFormat format)
        {
            //Image = ResizeImage(Image, 214, 108);
            Image.Save(stream, format);
        }
        public Bitmap Image { get; set; }
        public virtual int Width => Image.Width;
        public virtual int Height => Image.Height;
        public void Dispose()
        {
            Image.Dispose();
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
