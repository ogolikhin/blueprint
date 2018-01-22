using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections.Models;

namespace ArtifactStore.ArtifactList
{
    public interface IArtifactListService
    {
        Task<ProfileColumnsSettings> GetColumnSettingsAsync(int itemId, int userId);

        Task<int> SaveColumnsSettingsAsync(int itemId, ProfileColumnsSettings columnSettings, int userId);
    }
}
