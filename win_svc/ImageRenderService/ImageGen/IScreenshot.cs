using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageRenderService.ImageGen
{
    public interface IScreenshot : IDisposable
    {
        //public IScreenshot();

        //public Screenshot(Bitmap image);

        void Save(MemoryStream stream, ImageFormat format);
        Bitmap Image { get; set; }
        //public Size Size => Image.Size;
        int Width { get; }
        int Height { get; }
    }
}
