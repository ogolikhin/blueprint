using System.Threading.Tasks;
using ServiceLibrary.Models.Collection;

namespace ArtifactStore.Services.ArtifactListSettings
{
    public interface IArtifactListSettingsService
    {
        Task<int> SaveArtifactListColumnsSettingsAsync(
            int itemId, ProfileColumnsSettings profileColumnsSettings, int userId);
    }
}
