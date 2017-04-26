using System.Threading.Tasks;
using CefSharp.OffScreen;

namespace ImageRenderService.ImageGen
{
    public interface IBrowserPool
    {
        Task<ChromiumWebBrowser> Rent();
        void Return(ChromiumWebBrowser browser);
    }
}
