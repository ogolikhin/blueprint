using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    public interface ISqlArtifactRepository
    {
        Task<List<Artifact>> GetProjectOrArtifactChildrenAsync(int projectId, int? artifactId, int userId);

        Task<List<Artifact>> GetExpandedTreeToArtifactAsync(int projectId, int expandedToArtifactId, bool includeChildren, int userId);

        Task<IEnumerable<AuthorHistory>> GetAuthorHistories(IEnumerable<int> artifactIds);

        Task<IEnumerable<AuthorHistory>> GetAuthorHistoriesWithPermissionsCheck(IEnumerable<int> artifactIds, int userId);    

        Task<IEnumerable<SubArtifact>> GetSubArtifactTreeAsync(int artifactId, int userId, int revisionId = int.MaxValue, bool includeDrafts = true);

        Task<List<Artifact>> GetArtifactNavigationPathAsync(int artifactId, int userId);

        Task<IDictionary<int, IEnumerable<Artifact>>> GetArtifactsNavigationPathsAsync(
            int userId,
            IEnumerable<int> artifactIds,
            bool includeArtifactItself = true,
            int? revisionId = null,
            bool addDraft = true);

        Task<IEnumerable<ProjectNameIdPair>> GetProjectNameByIdsAsync(IEnumerable<int> projectIds);

        Task<IEnumerable<BaselineInfo>> GetBaselineInfo(IEnumerable<int> artifactIds, int userId, bool addDrafts, int revisionId);
    }
}