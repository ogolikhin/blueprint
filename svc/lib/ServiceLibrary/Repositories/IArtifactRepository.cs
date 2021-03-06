﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;

namespace ServiceLibrary.Repositories
{
    public interface IArtifactRepository
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

        Task<bool> IsArtifactLockedByUserAsync(int artifactId, int userId);

        Task<IEnumerable<ProcessInfoDto>> GetProcessInformationAsync(IEnumerable<int> artifactIds, int userId);

        Task<IEnumerable<StandardArtifactType>> GetStandardArtifactTypes(StandardArtifactTypes filter = StandardArtifactTypes.All);

        Task<ArtifactBasicDetails> GetArtifactBasicDetails(int artifactId, int userId, IDbTransaction transaction = null);

        Task<IEnumerable<PropertyType>> GetStandardProperties(ISet<int> standardArtifactTypeIds);

        Task<IReadOnlyList<ArtifactPropertyInfo>> GetArtifactsWithPropertyValuesAsync(int userId, IEnumerable<int> artifactIds,
            IEnumerable<int> propertyTypePredefineds, IEnumerable<int> propertyTypeIds);
    }
}