using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models.ProjectMeta;

namespace ServiceLibrary.Repositories.ProjectMeta
{
    public interface ISqlProjectMetaRepository
    {
        Task<ProjectTypes> GetCustomProjectTypesAsync(int projectId, int userId);

        Task<ProjectTypes> GetStandardProjectTypesAsync();

        Task<IEnumerable<ProjectApprovalStatus>> GetApprovalStatusesAsync(int projectId, int userId);

        Task<IEnumerable<PropertyType>> GetStandardProjectPropertyTypesAsync(IEnumerable<int> predefinedTypeIds);
    }
}