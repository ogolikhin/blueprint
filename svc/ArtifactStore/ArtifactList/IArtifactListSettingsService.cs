using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections.Models;

namespace ArtifactStore.ArtifactList
{
    public interface IArtifactListSettingsService
    {
        Task<int> SaveArtifactListColumnsSettingsAsync(
            int itemId, ProfileColumnsSettings profileColumnsSettings, int userId);
    }
}
