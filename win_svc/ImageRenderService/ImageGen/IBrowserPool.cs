using System.Threading.Tasks;
using CefSharp.OffScreen;

namespace ImageRenderService.ImageGen
{
    public interface IBrowserPool
    {
        Task<IVirtualBrowser> Rent();
        void Return(IVirtualBrowser browser);
    }
}
