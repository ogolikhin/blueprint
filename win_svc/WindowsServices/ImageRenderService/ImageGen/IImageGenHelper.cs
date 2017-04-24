using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageRenderService.ImageGen
{
    interface IImageGenHelper
    {
        Task<byte[]> GenerateImageAsync(string url, ImageFormat format);
    }
}
