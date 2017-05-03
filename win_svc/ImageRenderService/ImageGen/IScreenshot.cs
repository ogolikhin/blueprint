using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
namespace ImageRenderService.ImageGen
{
    public interface IScreenshot : IDisposable
    {
        void Save(Stream stream, ImageFormat format);
        Bitmap Image { get; set; }
        int Width { get; }
        int Height { get; }
    }
}
