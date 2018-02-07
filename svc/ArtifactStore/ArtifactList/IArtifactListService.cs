using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;

namespace ArtifactStore.ArtifactList
{
    public interface IArtifactListService
    {
        Task<int?> GetPaginationLimitAsync(int itemId, int userId);

        Task<ProfileColumns> GetProfileColumnsAsync(int itemId, int userId, ProfileColumns defaultColumns = null);

        Task<int> SavePaginationLimitAsync(int itemId, int? paginationLimit, int userId);

        Task<int> SaveProfileColumnsAsync(int itemId, ProfileColumns profileColumns, int userId);
    }
}
