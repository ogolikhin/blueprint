using System.Threading.Tasks;
using ArtifactStore.Models;
using System.Collections.Generic;

namespace ArtifactStore.Repositories
{
    public interface ISqlProjectMetaRepository
    {
        Task<ProjectTypes> GetCustomProjectTypesAsync(int projectId, int userId);
        Task<IEnumerable<ProjectApprovalStatus>> GetApprovalStatusesAsync(int projectId, int userId);
    }
}