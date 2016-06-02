using System.Threading.Tasks;
using ArtifactStore.Models;

namespace ArtifactStore.Repositories
{
    public interface ISqlProjectMetaRepository
    {
        Task<ProjectTypes> GetCustomProjectTypesAsync(int projectId, int userId);
    }
}