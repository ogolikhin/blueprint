using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace BlueprintSys.RC.ImageService.ImageGen
{
    public interface IScreenshot : IDisposable
    {
        void Save(Stream stream, ImageFormat format);
        Bitmap Image { get; set; }
        int Width { get; }
        int Height { get; }
    }
}
