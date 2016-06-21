﻿using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

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

    internal class OpenArtifactPermission
    {
        public int HolderId;
        public long? Permissions;
    }

    public class ArtifactItemProject
    {
        public int ProjectId;
        public int ArtifactId;
        public int ItemId;
    }

    public class SqlArtifactPermissionsRepository : IArtifactPermissionsRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        public SqlArtifactPermissionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }
        internal SqlArtifactPermissionsRepository(ISqlConnectionWrapper connectionWrapper)
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

        private async Task GetOpenArtifactPermissions(Dictionary<int, RolePermissions> itemIdsPermissions, IEnumerable<ProjectsArtifactsItem> projectIdsArtifactIdsItemIds, int sessionUserId, IEnumerable<int> projectArtifactIds, int? revisionId)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", sessionUserId);
            prm.Add("@artifactIds", DapperHelper.GetIntCollectionTableValueParameter(projectArtifactIds));
            prm.Add("@revisionId", revisionId ?? int.MaxValue);
            prm.Add("@addDrafts", (revisionId == null));
            var openArtifactPermissions = (await ConnectionWrapper.QueryAsync<OpenArtifactPermission>("GetOpenArtifactPermissions", prm, commandType: CommandType.StoredProcedure)).ToList();

            foreach (var openArtifactPermission in openArtifactPermissions)
            {
                foreach (var projectIdArtifactIdItemId in projectIdsArtifactIdsItemIds)
                {
                    if (projectIdArtifactIdItemId.HolderId == openArtifactPermission.HolderId && !itemIdsPermissions.Keys.Contains(projectIdArtifactIdItemId.VersionArtifactId))
                    {
                        itemIdsPermissions.Add(projectIdArtifactIdItemId.VersionArtifactId, (RolePermissions)openArtifactPermission.Permissions);
                    }
                }
            }
        }

        private async Task<bool> IsInstanceAdmin(IEnumerable<int> itemIds, bool contextUser, int sessionUserId)
        {
            var tvp = DapperHelper.GetIntCollectionTableValueParameter(itemIds);
            var prm = new DynamicParameters();
            prm.Add("@contextUser", contextUser);
            prm.Add("@userId", sessionUserId);
            return (await ConnectionWrapper.QueryAsync<bool>("NOVAIsInstanceAdmin", prm, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        private async Task<Tuple <IEnumerable<bool>, IEnumerable<ProjectsArtifactsItem>, IEnumerable<VersionProjectInfo>>> GetArtifactsProjects(IEnumerable<int> itemIds, int sessionUserId, int? revisionId, bool contextUser)
        {
            var prm = new DynamicParameters();
            prm.Add("@contextUser", contextUser);
            prm.Add("@userId", sessionUserId);
            prm.Add("@itemIds", DapperHelper.GetIntCollectionTableValueParameter(itemIds));
            prm.Add("@revisionId", (revisionId == null) ? int.MaxValue : revisionId); //HEAD revision
            prm.Add("@addDrafts", (revisionId == null));
           return (await ConnectionWrapper.QueryMultipleAsync<bool, ProjectsArtifactsItem, VersionProjectInfo>("GetArtifactsProjects", prm, commandType: CommandType.StoredProcedure));
        }

        public async Task<Dictionary<int, RolePermissions>> GetArtifactPermissions(IEnumerable<int> itemIds, int sessionUserId, bool contextUser = false, int? revisionId = null)
        {
            if (itemIds.Count() > 50)
            {
                throw new ArgumentOutOfRangeException("Cannot get artifact permissions for this many artifacts");
            }
            var isInstanceAdmin = await IsInstanceAdmin(itemIds, contextUser, sessionUserId);
            if (isInstanceAdmin)
            {
                var allPermissions = GetAllPermissions();
                return itemIds.ToDictionary(itemId => itemId, itemId => allPermissions); // RolePermissions.All
            }
            else
            {
                var multipleResult = await GetArtifactsProjects(itemIds, sessionUserId, revisionId, contextUser);
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

                    try
                    {
                        await GetOpenArtifactPermissions(itemIdsPermissions, projectsArtifactsItems, sessionUserId, projectArtifactIds, revisionId);
                    }
                    catch (SqlException sqle)
                    {
                        // 0x80131904: The statement terminated. The maximum recursion 100 has been exhausted before statement completion.
                        if (sqle.ErrorCode == -2146232060)
                        { // keeping this here for future optimization purposes
                            throw new HttpResponseException(HttpStatusCode.InternalServerError);
                        }
                    }
                }
                return itemIdsPermissions;
            }
        }

        public Task<ProjectPermissions> GetProjectPermissions(int projectId)
        {
            var discussionsPrm = new DynamicParameters();
            discussionsPrm.Add("@ProjectId", projectId);

            return  ConnectionWrapper.ExecuteScalarAsync<ProjectPermissions>("GetProjectPermissions", discussionsPrm, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ArtifactItemProject>> GetItemsInfos(int itemId, int userId, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var itemsPrm = new DynamicParameters();
            itemsPrm.Add("@itemId", itemId);
            itemsPrm.Add("@userId", userId);
            itemsPrm.Add("@addDrafts", addDrafts);
            itemsPrm.Add("@revisionId", revisionId);
            return (await ConnectionWrapper.QueryAsync<ArtifactItemProject>("GetItemsInfos", itemsPrm, commandType: CommandType.StoredProcedure));
        }
    }
}