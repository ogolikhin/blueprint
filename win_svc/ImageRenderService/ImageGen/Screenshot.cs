using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImageRenderService.ImageGen
{
    public sealed class Screenshot : IScreenshot
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
            Image.Save(stream, format);
        }
        public Bitmap Image { get; set; }
        public int Width => Image.Width;
        public int Height => Image.Height;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            Image.Dispose();
        }
    }
}
