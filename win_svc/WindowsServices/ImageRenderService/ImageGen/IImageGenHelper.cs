using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace ImageRenderService.ImageGen
{
    public interface IImageGenHelper
    {
        Task<byte[]> GenerateImageAsync(string url, ImageFormat format);
    }
}
