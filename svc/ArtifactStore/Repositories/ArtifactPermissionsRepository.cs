using ArtifactStore.Helpers;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ArtifactStore.Repositories
{
    internal class ProjectsArtifactsItem
    {
        public int VersionProjectId;
        public int VersionArtifactId;
        public int HolderId;
    }
    internal class VersionProjectInfo
    {
        public int ProjectId;
        public long? Permissions;
    }

    public class ArtifactPermissionsRepository : IArtifactPermissionsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        public ArtifactPermissionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }
        internal ArtifactPermissionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }
        private RolePermissions GetAllPermissions()
        {
            var allPermissions = RolePermissions.None;
            foreach (long permission in Enum.GetValues(typeof(RolePermissions)))
            {
                allPermissions |= (RolePermissions)permission;
            }
            return allPermissions;
        }

        public async Task<Dictionary<int, RolePermissions>> GetArtifactPermissions(IEnumerable<int> itemIds, int userId, bool contextUser = false, int? revisionId = null)
        {
            if (itemIds.Count() > 50)
            {
                throw new ArgumentOutOfRangeException("Cannot get artifact permissions for this many artifacts");
            }
            var tvp = DapperHelper.GetIntCollectionTableValueParameter(itemIds);
            var prm = new DynamicParameters();
            prm.Add("@contextUser", contextUser);
            prm.Add("@userId", userId);
            prm.Add("@itemIds", tvp);
            prm.Add("@revisionId", (revisionId == null) ? int.MaxValue : revisionId); //HEAD revision
            prm.Add("@addDrafts", (revisionId == null));
            var result = await ConnectionWrapper.QueryAsync<bool>("GetArtifactsProjects", prm, commandType: CommandType.StoredProcedure);

            var isInstanceAdmin = result.SingleOrDefault();
            if (isInstanceAdmin)
            {
                var allPermissions = GetAllPermissions();
                return itemIds.ToDictionary(itemId => itemId, itemId => allPermissions); // RolePermissions.All
            }
            else
            {
                var multipleResult = await ConnectionWrapper.QueryMultipleAsync<bool, ProjectsArtifactsItem, VersionProjectInfo>("GetArtifactsProjects", prm, commandType: CommandType.StoredProcedure);
                var projectsArtifactsItems = multipleResult.Item2.ToList();//???Do we need always do it
                var versionProjectInfos = multipleResult.Item3;

                var projectIds = new HashSet<int>(projectsArtifactsItems.Select(i => i.VersionProjectId));
                Dictionary<int, RolePermissions> itemIdsPermissions = new Dictionary<int, RolePermissions>(projectsArtifactsItems.Count);
                foreach (var projectInfo in versionProjectInfos)
                {
                    if (!projectInfo.Permissions.HasValue)
                    {
                        continue;
                    }
                    foreach (var projectArtifactItem in projectsArtifactsItems)
                    {
                        if (projectArtifactItem.VersionProjectId == projectInfo.ProjectId)
                        {
                            itemIdsPermissions.Add(projectArtifactItem.HolderId, (RolePermissions)projectInfo.Permissions);
                        }
                    }
                }

                ISet<int> projectArtifactIds = null;
                var projectOnlyScopeIdsPermissions = new HashSet<int>(versionProjectInfos.Select(i => i.ProjectId));
                foreach (int projectId in projectIds)
                {
                    if (projectOnlyScopeIdsPermissions.Contains(projectId))
                    {
                        continue;
                    }
                    if (projectArtifactIds == null)
                    {
                        projectArtifactIds = new HashSet<int>();
                    }
                    else
                    {
                        projectArtifactIds.Clear();
                    }
                    foreach (var projectArtifactItem in projectsArtifactsItems)
                    {
                        if (projectArtifactItem.VersionProjectId == projectId)
                        {
                            projectArtifactIds.Add(projectArtifactItem.VersionArtifactId);
                        }
                    }
                }
                return itemIdsPermissions;
            }
        }
    }
}