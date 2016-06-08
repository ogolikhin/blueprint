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
using static Dapper.SqlMapper;

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
            public long? Permissions;
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

        private ICustomQueryParameter GetIntCollectionTableValueParameter(DataTable dataTable)
        {
            return dataTable.AsTableValuedParameter("[dbo].[Int32Collection]");
        }

        public async Task<Dictionary<int, RolePermissions>> GetArtifactPermissions(IEnumerable<int> itemIds, int userId, bool contextUser = false, int? revisionId = null)
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

            var tvp = GetIntCollectionTableValueParameter(itemIdsTable);
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
                return itemIds.ToDictionary(itemId => itemId, itemId =>GetAllPermissions()); // RolePermissions.All
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