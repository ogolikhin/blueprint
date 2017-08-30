using System.Threading.Tasks;

namespace BlueprintSys.RC.ImageService.ImageGen
{
    public interface IBrowserPool
    {
        Task<IVirtualBrowser> Rent();
        void Return(IVirtualBrowser browser);
    }
}
