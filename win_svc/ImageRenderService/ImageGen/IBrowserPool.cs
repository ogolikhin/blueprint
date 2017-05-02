using System.Threading.Tasks;

namespace ImageRenderService.ImageGen
{
    public interface IBrowserPool
    {
        Task<IVirtualBrowser> Rent();
        void Return(IVirtualBrowser browser);
    }
}
