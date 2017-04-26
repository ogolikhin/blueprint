using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace ImageRenderService.ImageGen
{
    public interface IImageGenHelper
    {
        Task<MemoryStream> GenerateImageAsync(string url, ImageFormat format);
    }
}
