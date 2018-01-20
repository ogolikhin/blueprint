using System.Threading.Tasks;
using ServiceLibrary.Models.Collection;

namespace ArtifactStore.Services.ArtifactListSettings
{
    public interface IArtifactListSettingsService
    {
        Task<int> SaveArtifactListColumnsSettings(int itemId, int userId,
            ArtifactListColumnsSettings artifactListColumnsSettings);
    }
}
