using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;

namespace ArtifactStore.ArtifactList
{
    public interface IArtifactListService
    {
        Task<ProfileColumns> GetProfileColumnsAsync(int itemId, int userId, ProfileColumns defaultColumns = null);

        Task<int> SaveProfileColumnsAsync(int itemId, ProfileColumns profileColumns, int userId);
    }
}
