using System.Threading.Tasks;
using ArtifactStore.ArtifactList.Models;

namespace ArtifactStore.ArtifactList
{
    public interface IArtifactListService
    {
        Task<ProfileSettings> GetProfileSettingsAsync(int itemId, int userId);

        Task<int?> GetPaginationLimitAsync(int itemId, int userId);

        Task<ProfileColumns> GetProfileColumnsAsync(int itemId, int userId, ProfileColumns defaultColumns);

        Task<ProfileColumns> GetProfileColumnsAsync(int itemId, int userId);

        Task SaveProfileSettingsAsync(int itemId, int userId, ProfileColumns profileColumns, int? paginationLimit);

        Task<int> SavePaginationLimitAsync(int itemId, int? paginationLimit, int userId);

        Task<int> SaveProfileColumnsAsync(int itemId, ProfileColumns profileColumns, int userId);
    }
}
