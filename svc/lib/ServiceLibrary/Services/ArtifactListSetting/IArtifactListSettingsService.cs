using System.Threading.Tasks;
using ServiceLibrary.Models.Collection;

namespace ServiceLibrary.Services.ArtifactListSetting
{
    public interface IArtifactListSettingsService
    {
        Task<int> SaveArtifactColumnsSettings(int itemId, int userId, ArtifactListSettings artifactListSettings);
    }
}
