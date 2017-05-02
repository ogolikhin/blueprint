﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    public class SqlInstanceRepository : IInstanceRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        public SqlInstanceRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlInstanceRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        public async Task<InstanceItem> GetInstanceFolderAsync(int folderId, int userId)
        {
            if (folderId < 1)
                throw new ArgumentOutOfRangeException(nameof(folderId));

            var prm = new DynamicParameters();
            prm.Add("@folderId", folderId);
            prm.Add("@userId", userId);

            var folder = (await ConnectionWrapper.QueryAsync<InstanceItem>("GetInstanceFolderById", prm, commandType: CommandType.StoredProcedure))?.FirstOrDefault();
            if(folder == null)
                throw new ResourceNotFoundException(string.Format("Instance Folder (Id:{0}) is not found.", folderId), ErrorCodes.ResourceNotFound);

            folder.Type = InstanceItemTypeEnum.Folder;
            return folder;
        }

        public async Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int folderId, int userId)
        {
            if (folderId < 1)
                throw new ArgumentOutOfRangeException(nameof(folderId));
            if (userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var prm = new DynamicParameters();
            prm.Add("@folderId", folderId);
            prm.Add("@userId", userId);

            return  ((await ConnectionWrapper.QueryAsync<InstanceItem>("GetInstanceFolderChildren", prm, commandType: CommandType.StoredProcedure))
                ?? Enumerable.Empty <InstanceItem>()).OrderBy(i => i.Type).ThenBy(i => i.Name).ToList();
        }

        public async Task<InstanceItem> GetInstanceProjectAsync(int projectId, int userId)
        {
            if (projectId < 1)
                throw new ArgumentOutOfRangeException(nameof(projectId));
            if (userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var prm = new DynamicParameters();
            prm.Add("@projectId", projectId);
            prm.Add("@userId", userId);

            var project = (await ConnectionWrapper.QueryAsync<InstanceItem>("GetInstanceProjectById", prm, commandType: CommandType.StoredProcedure))?.FirstOrDefault();
            if (project == null)
                throw new ResourceNotFoundException(string.Format("Project (Id:{0}) is not found.", projectId), ErrorCodes.ResourceNotFound);
            if(!project.IsAccesible.GetValueOrDefault())
                throw new AuthorizationException(string.Format("The user does not have permissions for Project (Id:{0}).", projectId), ErrorCodes.UnauthorizedAccess);
            
            return project;
        }

        public async Task<List<string>> GetProjectNavigationPathAsync(int projectId, int userId, bool includeProjectItself = true)
        {
            if (projectId < 1)
                throw new ArgumentOutOfRangeException(nameof(projectId));
            if (userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@projectId", projectId);

            var projectPaths = (await ConnectionWrapper.QueryAsync<ArtifactsNavigationPath>("GetProjectNavigationPath", param, commandType: CommandType.StoredProcedure)).ToList();
            if(projectPaths.Count == 0)
                throw new ResourceNotFoundException($"The project (Id:{projectId}) can no longer be accessed. It may have been deleted, or is no longer accessible by you.", ErrorCodes.ResourceNotFound);
            if (!includeProjectItself)
                projectPaths.RemoveAll(p => p.Level == 0);

            return projectPaths.OrderByDescending(p => p.Level).Select(p => p.Name).ToList();
        }

        public async Task<IEnumerable<AdminRole>> GetInstanceRolesAsync()
        {
            var result = await ConnectionWrapper.QueryAsync<AdminRole>("GetInstanceAdminRoles", commandType: CommandType.StoredProcedure);
            return result;
        }
    }
}