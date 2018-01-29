using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;

namespace ArtifactStore.ArtifactList
{
    public interface IArtifactListService
    {
        Task<ProfileColumns> GetProfileColumnsAsync(int itemId, int userId);

        Task<int> SaveProfileColumnsAsync(int itemId, ProfileColumns profileColumns, int userId);
    }
}
