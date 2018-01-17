﻿using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace ServiceLibrary.Repositories
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

    public class OpenArtifactPermission
    {
        public int HolderId;
        public long? Permissions;
    }

    internal class ItemRawData
    {
        public int ItemId;
        public string RawData;
    }

    public class SqlArtifactPermissionsRepository : IArtifactPermissionsRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlArtifactPermissionsRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlArtifactPermissionsRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
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

        private async Task GetOpenArtifactPermissions(Dictionary<int, RolePermissions> itemIdsPermissions, IEnumerable<ProjectsArtifactsItem> projectIdsArtifactIdsItemIds, int sessionUserId, IEnumerable<int> projectArtifactIds, int revisionId = int.MaxValue, bool addDrafts = true, IDbTransaction transaction = null)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", sessionUserId);
            prm.Add("@artifactIds", SqlConnectionWrapper.ToDataTable(projectArtifactIds, "Int32Collection", "Int32Value"));

            List<OpenArtifactPermission> openArtifactPermissions = null;

            if (transaction == null)
            {
                openArtifactPermissions = (await _connectionWrapper.QueryAsync<OpenArtifactPermission>("GetOpenArtifactPermissions", prm, commandType: CommandType.StoredProcedure)).ToList();
            }
            else
            {
                openArtifactPermissions = (await transaction.Connection.QueryAsync<OpenArtifactPermission>("GetOpenArtifactPermissions", prm, transaction, commandType: CommandType.StoredProcedure)).ToList();
            }

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

        private async Task<bool> IsInstanceAdmin(bool contextUser, int sessionUserId, IDbTransaction transaction = null)
        {
            var prm = new DynamicParameters();
            prm.Add("@contextUser", contextUser);
            prm.Add("@userId", sessionUserId);

            if (transaction == null)
            {
                return (await _connectionWrapper.QueryAsync<bool>("IsInstanceAdmin", prm, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            }
            else
            {
                return (await transaction.Connection.QueryAsync<bool>("IsInstanceAdmin", prm, transaction, commandType: CommandType.StoredProcedure)).SingleOrDefault();
            }
        }

        private async Task<Tuple<IEnumerable<ProjectsArtifactsItem>, IEnumerable<VersionProjectInfo>>> GetArtifactsProjects(IEnumerable<int> itemIds, int sessionUserId, int revisionId, bool addDrafts, IDbTransaction transaction = null)
        {
            var prm = new DynamicParameters();
            prm.Add("@userId", sessionUserId);
            prm.Add("@itemIds", SqlConnectionWrapper.ToDataTable(itemIds, "Int32Collection", "Int32Value"));

            if (transaction == null)
            {
                return (await _connectionWrapper.QueryMultipleAsync<ProjectsArtifactsItem, VersionProjectInfo>("GetArtifactsProjects", prm, commandType: CommandType.StoredProcedure));
            }
            else
            {
                // return (await _connectionWrapper.QueryMultipleAsync<ProjectsArtifactsItem, VersionProjectInfo>("GetArtifactsProjects", prm, commandType: CommandType.StoredProcedure));

                using (var command = await transaction.Connection.QueryMultipleAsync("GetArtifactsProjects", prm, transaction, commandType: CommandType.StoredProcedure))
                {
                    var projectsAtifacts = command.Read<ProjectsArtifactsItem>().ToList();
                    var versionProjects = command.Read<VersionProjectInfo>().ToList();

                    return new Tuple<IEnumerable<ProjectsArtifactsItem>, IEnumerable<VersionProjectInfo>>(projectsAtifacts, versionProjects);
                }
            }
        }

        public async Task<Dictionary<int, RolePermissions>> GetArtifactPermissions(IEnumerable<int> itemIds, int sessionUserId, bool contextUser = false, int revisionId = int.MaxValue, bool addDrafts = true, IDbTransaction transaction = null)
        {
            var itemIdsList = itemIds is List<int> ? (List<int>)itemIds : itemIds.ToList();
            var dictionary = new Dictionary<int, RolePermissions>();
            int index = 0;

            while (index < itemIdsList.Count)
            {
                int chunkSize = 50;
                if (chunkSize > itemIdsList.Count - index)
                {
                    chunkSize = itemIdsList.Count - index;
                }

                var chunk = itemIdsList.GetRange(index, chunkSize);
                var localResult = await GetArtifactPermissionsInternal(chunk, sessionUserId, contextUser, revisionId, addDrafts, transaction);
                dictionary = dictionary.Union(localResult).ToDictionary(k => k.Key, v => v.Value);
                index += chunkSize;
            }

            return dictionary;
        }

        public async Task<bool> HasReadPermissions(int artifactId, int sessionUserId, bool contextUser = false, int revisionId = int.MaxValue, bool addDrafts = true)
        {
            var result = await GetArtifactPermissions(new[] { artifactId }, sessionUserId, contextUser, revisionId, addDrafts);
            RolePermissions permission;

            return result.TryGetValue(artifactId, out permission) && permission.HasFlag(RolePermissions.Read);
        }

        public async Task<bool> HasEditPermissions(int artifactId, int sessionUserId, bool contextUser = false, int revisionId = int.MaxValue, bool addDrafts = true, IDbTransaction transaction = null)
        {
            var result = await GetArtifactPermissions(new[] { artifactId }, sessionUserId, contextUser, revisionId, addDrafts, transaction);
            RolePermissions permission;

            return result.TryGetValue(artifactId, out permission) && permission.HasFlag(RolePermissions.Edit);
        }

        private async Task<Dictionary<int, RolePermissions>> GetArtifactPermissionsInternal(IEnumerable<int> itemIds, int sessionUserId, bool contextUser = false, int revisionId = int.MaxValue, bool addDrafts = true, IDbTransaction transaction = null)
        {
            if (itemIds.Count() > 50)
            {
                throw new ArgumentOutOfRangeException("Cannot get artifact permissions for this many artifacts");
            }

            var isInstanceAdmin = await IsInstanceAdmin(contextUser, sessionUserId, transaction);
            if (isInstanceAdmin)
            {
                var allPermissions = GetAllPermissions();
                return itemIds.ToDictionary(itemId => itemId, itemId => allPermissions); // RolePermissions.All
            }

            var multipleResult = await GetArtifactsProjects(itemIds, sessionUserId, revisionId, addDrafts, transaction);
            var projectsArtifactsItems = multipleResult.Item1.ToList(); // ???Do we need always do it
            var versionProjectInfos = multipleResult.Item2;

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
                    await GetOpenArtifactPermissions(itemIdsPermissions, projectsArtifactsItems, sessionUserId, projectArtifactIds, revisionId, addDrafts, transaction);
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

        public Task<ProjectPermissions> GetProjectPermissions(int projectId)
        {
            var discussionsPrm = new DynamicParameters();
            discussionsPrm.Add("@ProjectId", projectId);

            return _connectionWrapper.ExecuteScalarAsync<ProjectPermissions>("GetProjectPermissions", discussionsPrm, commandType: CommandType.StoredProcedure);
        }

        public async Task<ItemInfo> GetItemInfo(int itemId, int userId, bool addDrafts = true, int revisionId = int.MaxValue)
        {
            var itemsPrm = new DynamicParameters();
            itemsPrm.Add("@itemId", itemId);
            itemsPrm.Add("@userId", userId);
            itemsPrm.Add("@addDrafts", addDrafts);
            itemsPrm.Add("@revisionId", revisionId);

            return (await _connectionWrapper.QueryAsync<ItemInfo>("GetItemInfo", itemsPrm, commandType: CommandType.StoredProcedure)).SingleOrDefault();
        }

        public static bool HasPermissions(int itemId, Dictionary<int, RolePermissions> permissions, RolePermissions permissionType)
        {
            RolePermissions permission;

            return permissions.TryGetValue(itemId, out permission) && permission.HasFlag(permissionType);
        }

        // Until we completely port over all permission calculation functionality over from Raptor,
        // we will encounter scenarios where our current method of permission calculation is not
        // sufficient. Until then, we will directly get the user's permission for an artifact
        // directly. STOR-6440
        public async Task<Dictionary<int, RolePermissions>> GetArtifactPermissionDirectly(int itemId, int userId, int projectId)
        {
            var sqlParameters = new DynamicParameters();
            sqlParameters.Add("@userId", userId);
            sqlParameters.Add("@projectId", projectId);
            sqlParameters.Add("@artifactId", itemId);

            string sqlString = @"SELECT [Perm] FROM [dbo].[GetArtifactPermission](@userId,@projectId,@artifactId)";
            var hasPermission = (await _connectionWrapper.QueryAsync<bool>(sqlString, sqlParameters, commandType: CommandType.Text)).FirstOrDefault();

            var itemPermission = hasPermission ? RolePermissions.Read : RolePermissions.None;

            var result = new Dictionary<int, RolePermissions>();
            result.Add(itemId, itemPermission);

            return result;
        }
    }
}
