using ArtifactStore.Models;
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
    public class SqlArtifactVersionsRepository : ISqlArtifactVersionsRepository
    {
        public class ProjectsArtifactsItem
        {
            public int VersionProjectId;
            public int VersionArtifactId;
            public int HolderId;
        }

        public class VersionProjectInfo
        {
            public int ProjectId;
            public long Permissions;
        }
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        public SqlArtifactVersionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlArtifactVersionsRepository(ISqlConnectionWrapper connectionWrapper)
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

        public async Task<Dictionary<int, RolePermissions>> GetArtifactPermissions(IEnumerable<int> itemIds, int userId, bool contextUser = true, int? revisionId = null)
        {
            if (itemIds.Count() > 50)
            {
                throw new ArgumentOutOfRangeException("Cannot get artifact permissions for this many artifacts");
            }
            DataTable itemIdsTable = new DataTable();
            itemIdsTable.Columns.Add("Int32Value", typeof(int));
            foreach (int itemId in itemIds)
            {
                itemIdsTable.Rows.Add(itemId);
            }
            var tvp = itemIdsTable.AsTableValuedParameter("[dbo].[Int32Collection]");
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
                return itemIds.ToDictionary(itemId => itemId, itemId => GetAllPermissions()); // RolePermissions.All
            }
            else
            {
                ISet<int> projectIds = null;
                IList<Tuple<int, int, int>> projectIdsArtifactIdsItemIds = null;
                IDictionary<int, RolePermissions?> projectOnlyScopeIdsPermissions = null;
                var multipleResult = await ConnectionWrapper.QueryMultipleAsync<bool, ProjectsArtifactsItem, VersionProjectInfo>("GetArtifactsProjects", prm, commandType: CommandType.StoredProcedure);
                var projectsArtifactsItems = multipleResult.Item2;
                var versionProjectInfos = multipleResult.Item3;

                projectIds = new HashSet<int>();
                projectIdsArtifactIdsItemIds = new List<Tuple<int, int, int>>(itemIds.Count());
                foreach (var projectsArtifactsItem in projectsArtifactsItems)
                {
                    int versionProjectId = projectsArtifactsItem.VersionProjectId;
                    int versionArtifactId = projectsArtifactsItem.VersionArtifactId;
                    int holderId = projectsArtifactsItem.HolderId;
                    projectIds.Add(versionProjectId);
                    projectIdsArtifactIdsItemIds.Add(new Tuple<int, int, int>(versionProjectId, versionArtifactId, holderId));
                }

                projectOnlyScopeIdsPermissions = new Dictionary<int, RolePermissions?>();
                foreach (var versionProjectInfo in versionProjectInfos)
                {
                    int projectId = versionProjectInfo.ProjectId;
                    RolePermissions? permissions = (RolePermissions?)versionProjectInfo.Permissions;
                    projectOnlyScopeIdsPermissions.Add(projectId, permissions);
                }

                Dictionary<int, RolePermissions> itemIdsPermissions = new Dictionary<int, RolePermissions>(projectIdsArtifactIdsItemIds.Count);
                foreach (KeyValuePair<int, RolePermissions?> projectIdPermissions in projectOnlyScopeIdsPermissions)
                {
                    if (!projectIdPermissions.Value.HasValue)
                    {
                        continue;
                    }
                    foreach (Tuple<int, int, int> projectIdArtifactIdItemId in projectIdsArtifactIdsItemIds)
                    {
                        if (projectIdArtifactIdItemId.Item1 == projectIdPermissions.Key)
                        {
                            itemIdsPermissions.Add(projectIdArtifactIdItemId.Item3, projectIdPermissions.Value.Value);
                        }
                    }
                }

                ISet<int> projectArtifactIds = null;
                foreach (int projectId in projectIds)
                {
                    if (projectOnlyScopeIdsPermissions.ContainsKey(projectId))
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
                    foreach (Tuple<int, int, int> projectIdArtifactIdItemId in projectIdsArtifactIdsItemIds)
                    {
                        if (projectIdArtifactIdItemId.Item1 == projectId)
                        {
                            projectArtifactIds.Add(projectIdArtifactIdItemId.Item2);
                        }
                    }
                }
                return itemIdsPermissions;
            }
        }

        public async Task<ArtifactHistoryResultSet> GetArtifactVersions(int artifactId, int limit, int offset, int? userId, bool asc)
        {
            if (artifactId < 1)
                throw new ArgumentOutOfRangeException(nameof(artifactId));
            if (limit < 1 || limit > 100)
                throw new ArgumentOutOfRangeException(nameof(limit));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (userId.HasValue && userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));
            var prm = new DynamicParameters();
            prm.Add("@artifactId", artifactId);
            prm.Add("@lim", limit);
            prm.Add("@offset", offset);
            if (userId.HasValue)
            {
                prm.Add("@userId", userId.Value);
            }
            else
            {
                prm.Add("@userId", null);
            }
            prm.Add("@ascd", asc);
            var artifactVersions = (await ConnectionWrapper.QueryAsync<ArtifactHistoryVersion>("GetArtifactVersions", prm,
                    commandType: CommandType.StoredProcedure)).ToList();
            var result = new ArtifactHistoryResultSet {
                ArtifactId = artifactId,
                ArtifactHistoryVersions = artifactVersions
            };
            return result;
        }
    }
}