using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Models.DTO;
using AdminStore.Models.Enums;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AdminStore.Repositories
{
    public class SqlInstanceRepository : IInstanceRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlInstanceRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlInstanceRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        #region folders

        public async Task<InstanceItem> GetInstanceFolderAsync(int folderId, int userId, bool fromAdminPortal = false)
        {
            if (folderId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(folderId));
            }

            var parameters = new DynamicParameters();
            parameters.Add("@folderId", folderId);
            parameters.Add("@userId", userId);
            parameters.Add("@fromAdminPortal", fromAdminPortal);

            var folder = (await _connectionWrapper.QueryAsync<InstanceItem>("GetInstanceFolderById", parameters, commandType: CommandType.StoredProcedure))?.FirstOrDefault();
            if (folder == null)
            {
                throw new ResourceNotFoundException(string.Format("Instance Folder (Id:{0}) is not found.", folderId), ErrorCodes.ResourceNotFound);
            }

            folder.Type = InstanceItemTypeEnum.Folder;

            return folder;
        }

        public async Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int folderId, int userId, bool fromAdminPortal = false)
        {
            if (folderId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(folderId));
            }

            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            var prm = new DynamicParameters();
            prm.Add("@folderId", folderId);
            prm.Add("@userId", userId);
            prm.Add("@fromAdminPortal", fromAdminPortal);

            return ((await _connectionWrapper.QueryAsync<InstanceItem>("GetInstanceFolderChildren", prm, commandType: CommandType.StoredProcedure))
                    ?? Enumerable.Empty<InstanceItem>()).OrderBy(i => i.Type).ThenBy(i => i.Name).ToList();
        }

        public async Task<int> CreateFolderAsync(FolderDto folder)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Name", folder.Name);
            parameters.Add("@ParentFolderId", folder.ParentFolderId);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var folderId = await _connectionWrapper.ExecuteScalarAsync<int>("CreateFolder", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfCreatingFolder);

                    case (int)SqlErrorCodes.FolderWithSuchNameExistsInParentFolder:
                        throw new ConflictException(ErrorMessages.FolderWithSuchNameExistsInParentFolder, ErrorCodes.Conflict);

                    case (int)SqlErrorCodes.ParentFolderNotExists:
                        throw new ResourceNotFoundException(ErrorMessages.ParentFolderNotExists, ErrorCodes.ResourceNotFound);

                    default:
                        return folderId;
                }
            }
            return folderId;
        }

        public async Task<IEnumerable<InstanceItem>> GetFoldersByName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = UsersHelper.ReplaceWildcardCharacters(name);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@name", name);

            return await _connectionWrapper.QueryAsync<InstanceItem>
            (
                "GetFoldersByName",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> DeleteInstanceFolderAsync(int instanceFolderId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@InstanceFolderId", instanceFolderId);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.ExecuteScalarAsync<int>("DeleteFolder", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.InstanceFolderContainsChildrenItems:
                        throw new ConflictException(ErrorMessages.ErrorOfDeletingFolderThatContainsChildrenItems);
                }
            }

            if (result == 0)
            {
                throw new ResourceNotFoundException(ErrorMessages.FolderNotExist, ErrorCodes.ResourceNotFound);
            }

            return result;
        }

        public async Task UpdateFolderAsync(int folderId, FolderDto folderDto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@newFolderName", folderDto.Name);
            parameters.Add("@folderId", folderId);
            parameters.Add("@newParentFolderId", folderDto.ParentFolderId);

            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _connectionWrapper.ExecuteScalarAsync<int>("UpdateFolder", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfUpdatingFolder);

                    case (int)SqlErrorCodes.EditRootFolderIsForbidden:
                        throw new BadRequestException(ErrorMessages.EditRootFolderIsForbidden, ErrorCodes.BadRequest);

                    case (int)SqlErrorCodes.FolderWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.FolderNotExist, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.FolderWithSuchNameExistsInParentFolder:
                        throw new ConflictException(ErrorMessages.FolderWithSuchNameExistsInParentFolder, ErrorCodes.Conflict);

                    case (int)SqlErrorCodes.ParentFolderNotExists:
                        throw new ResourceNotFoundException(ErrorMessages.ParentFolderNotExists, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.ParentFolderIdReferenceToDescendantItem:
                        throw new ConflictException(ErrorMessages.ParentFolderIdReferenceToDescendantItem, ErrorCodes.Conflict);
                }
            }
        }

        #endregion

        #region projects

        public async Task<InstanceItem> GetInstanceProjectAsync(int projectId, int userId, bool fromAdminPortal = false)
        {
            if (projectId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            var prm = new DynamicParameters();
            prm.Add("@projectId", projectId);
            prm.Add("@userId", userId);

            var sqlQueryProcedure = fromAdminPortal ? "GetProjectDetails" : "GetInstanceProjectById";

            var project = (await _connectionWrapper.QueryAsync<InstanceItem>(sqlQueryProcedure, prm, commandType: CommandType.StoredProcedure))?.FirstOrDefault();
            if (project == null)
            {
                throw new ResourceNotFoundException(string.Format("Project (Id:{0}) is not found.", projectId), ErrorCodes.ResourceNotFound);
            }

            if (!project.IsAccesible.GetValueOrDefault())
            {
                throw new AuthorizationException(string.Format("The user does not have permissions for Project (Id:{0}).", projectId), ErrorCodes.UnauthorizedAccess);
            }

            return project;
        }

        public async Task<List<string>> GetProjectNavigationPathAsync(int projectId, int userId, bool includeProjectItself = true)
        {
            if (projectId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            var param = new DynamicParameters();
            param.Add("@userId", userId);
            param.Add("@projectId", projectId);

            var projectPaths = (await _connectionWrapper.QueryAsync<ArtifactsNavigationPath>("GetProjectNavigationPath", param, commandType: CommandType.StoredProcedure)).ToList();
            if (projectPaths.Count == 0)
            {
                throw new ResourceNotFoundException($"The project (Id:{projectId}) can no longer be accessed. It may have been deleted, or is no longer accessible by you.", ErrorCodes.ResourceNotFound);
            }

            if (!includeProjectItself)
            {
                projectPaths.RemoveAll(p => p.Level == 0);
            }

            return projectPaths.OrderByDescending(p => p.Level).Select(p => p.Name).ToList();
        }

        public async Task DeleteProject(int userId, int projectId)
        {
            // We need to check if project is still exist in database and not makred as deleted
            // Also we need to get the latest projectstatus to apply the right delete method
            ProjectStatus? projectStatus;

            InstanceItem project = await GetInstanceProjectAsync(projectId, userId, fromAdminPortal: true);

            if (!TryGetProjectStatusIfProjectExist(project, out projectStatus))
            {
                throw new ResourceNotFoundException(I18NHelper.FormatInvariant(ErrorMessages.ProjectWasDeletedByAnotherUser, project.Id, project.Name), ErrorCodes.ResourceNotFound);
            }

            if (projectStatus == ProjectStatus.Live)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userId", userId);
                parameters.Add("@projectId", projectId);

                await _connectionWrapper.ExecuteAsync("RemoveProject", parameters,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                var parameters = new DynamicParameters();
                parameters.Add("@projectId", projectId);
                parameters.Add("@result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await _connectionWrapper.ExecuteScalarAsync<int>("PurgeProject", parameters,
                    commandType: CommandType.StoredProcedure);
                var errorCode = parameters.Get<int?>("result");

                if (errorCode.HasValue)
                {
                    switch (errorCode.Value)
                    {
                        case -2: // Instance project issue
                            throw new ConflictException(I18NHelper.FormatInvariant(ErrorMessages.ForbidToPurgeSystemInstanceProjectForInternalUseOnly, project.Id), ErrorCodes.Conflict);
                        case -1: // Cross project move issue
                            throw new ResourceNotFoundException(I18NHelper.FormatInvariant(ErrorMessages.ArtifactWasMovedToAnotherProject, project.Id), ErrorCodes.ResourceNotFound);
                        case 0:
                            // Success
                            break;
                        default:
                            throw new Exception(ErrorMessages.GeneralErrorOfUpdatingProject);
                    }
                }
            }
        }

        public async Task<QueryResult<ProjectFolderSearchDto>> GetProjectsAndFolders(int userId, TabularData tabularData, Func<Sorting, string> sort = null)
        {
            var orderField = string.Empty;
            if (sort != null && tabularData.Sorting != null)
            {
                orderField = sort(tabularData.Sorting);
            }

            if (!string.IsNullOrWhiteSpace(tabularData.Search))
            {
                tabularData.Search = UsersHelper.ReplaceWildcardCharacters(tabularData.Search);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);
            parameters.Add("@Offset", tabularData.Pagination.Offset);
            parameters.Add("@Limit", tabularData.Pagination.Limit);
            parameters.Add("@OrderField", orderField);
            parameters.Add("@Search", tabularData.Search);
            parameters.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var projectFolders =
                await
                    _connectionWrapper.QueryAsync<ProjectFolderSearchDto>("SearchProjectsAndFolders", parameters,
                        commandType: CommandType.StoredProcedure);

            var total = parameters.Get<int?>("Total");

            var queryDataResult = new QueryResult<ProjectFolderSearchDto> { Items = projectFolders, Total = total ?? 0 };

            return queryDataResult;
        }

        #endregion

        #region roles

        public async Task<IEnumerable<AdminRole>> GetInstanceRolesAsync()
        {
            var result = await _connectionWrapper.QueryAsync<AdminRole>("GetInstanceAdminRoles", commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<IEnumerable<ProjectRole>> GetProjectRolesAsync(int projectId)
        {
            if (projectId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            var prm = new DynamicParameters();
            prm.Add("@projectId", projectId);
            prm.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = (await _connectionWrapper.QueryAsync<ProjectRole>("GetProjectRoles", prm, commandType: CommandType.StoredProcedure));

            var errorCode = prm.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.ProjectWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.ProjectNotExist, ErrorCodes.ResourceNotFound);

                }
            }

            return result;
        }

        public async Task<RoleAssignmentQueryResult<RoleAssignment>> GetProjectRoleAssignmentsAsync(int projectId, TabularData tabularData, Func<Sorting, string> sort = null)
        {
            var orderField = string.Empty;
            if (sort != null && tabularData.Sorting != null)
            {
                orderField = sort(tabularData.Sorting);
            }

            if (!string.IsNullOrWhiteSpace(tabularData.Search))
            {
                tabularData.Search = UsersHelper.ReplaceWildcardCharacters(tabularData.Search);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@projectId", projectId);
            parameters.Add("@Offset", tabularData.Pagination.Offset);
            parameters.Add("@Limit", tabularData.Pagination.Limit);
            parameters.Add("@OrderField", orderField);
            parameters.Add("@Search", tabularData.Search);
            parameters.Add("@Total", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@ProjectName", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var rolesAssigments = await _connectionWrapper.QueryAsync<RoleAssignment>("GetProjectRoleAssignments", parameters, commandType: CommandType.StoredProcedure);

            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue && errorCode.Value == (int)SqlErrorCodes.ProjectWithCurrentIdNotExist)
            {
                throw new ResourceNotFoundException(ErrorMessages.ProjectNotExist, ErrorCodes.ResourceNotFound);
            }

            var total = parameters.Get<int?>("Total");
            var projectName = parameters.Get<string>("ProjectName");

            var queryDataResult = new RoleAssignmentQueryResult<RoleAssignment> { Items = rolesAssigments, Total = total ?? 0, ProjectName = projectName ?? string.Empty};

            return queryDataResult;
        }

        public async Task<int> DeleteRoleAssignmentsAsync(int projectId, OperationScope scope, string search)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = UsersHelper.ReplaceWildcardCharacters(search);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId);
            parameters.Add("@RoleAssignmentIds", SqlConnectionWrapper.ToDataTable(scope.Ids));
            parameters.Add("@Search", search);
            parameters.Add("@SelectAll", scope.SelectAll);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.ExecuteScalarAsync<int>("DeleteProjectRoleAssigments", parameters,
                commandType: CommandType.StoredProcedure);

            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.ProjectWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.ProjectNotExist, ErrorCodes.ResourceNotFound);
                }
            }

            return result;
        }

        public async Task<int> HasProjectExternalLocksAsync(int userId, int projectId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@projectId", projectId);

            var hasProjectExternalLocksAsync = await _connectionWrapper.ExecuteScalarAsync<int>("IsProjectHasForeignLocks", parameters, commandType: CommandType.StoredProcedure);
            
            return hasProjectExternalLocksAsync;
        }

        public async Task<int> CreateRoleAssignmentAsync(int projectId, RoleAssignmentDTO roleAssignment)
        {
            if (projectId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            if (roleAssignment == null)
            {
                throw new ArgumentOutOfRangeException(nameof(roleAssignment));
            }

            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId);
            parameters.Add("@GroupId", roleAssignment.GroupId);
            parameters.Add("@RoleId", roleAssignment.RoleId);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _connectionWrapper.ExecuteScalarAsync<int>("CreateProjectRoleAssignment", parameters,
                commandType: CommandType.StoredProcedure);

            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.ProjectWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.ProjectNotExist, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.GroupWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.GroupIsNotFound, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.RolesForProjectNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.RoleIsNotFound, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.RoleAssignmentAlreadyExists:
                        throw new ConflictException(ErrorMessages.RoleAssignmentAlreadyExists, ErrorCodes.Conflict);
                }
            }

            return result;
        }

        public async Task UpdateRoleAssignmentAsync(int projectId, int roleAssignmentId, RoleAssignmentDTO roleAssignment)
        {
            if (projectId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            if (roleAssignment == null)
            {
                throw new ArgumentOutOfRangeException(nameof(roleAssignment));
            }
            
            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId);
            parameters.Add("@GroupId", roleAssignment.GroupId);
            parameters.Add("@RoleId", roleAssignment.RoleId);
            parameters.Add("@RoleAssignmentId", roleAssignmentId);
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _connectionWrapper.ExecuteScalarAsync<int>("UpdateProjectRoleAssigment", parameters,
                commandType: CommandType.StoredProcedure);

            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfUpdatingRoleAssignment);

                    case (int)SqlErrorCodes.ProjectWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.ProjectNotExist, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.GroupWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.GroupIsNotFound, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.RolesForProjectNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.RoleIsNotFound, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.RoleAssignmentNotExists:
                        throw new ResourceNotFoundException(ErrorMessages.RoleAssignmentNotFound, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.RoleAssignmentAlreadyExists:
                        throw new ConflictException(ErrorMessages.RoleAssignmentAlreadyExists, ErrorCodes.Conflict);

                }
            }
        }



        #endregion

        #region private methods


        /// <summary>
        ///  This method takes the projectId and checks if the project is still exist in the database and not marked as deleted
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="projectStatus">If the project exists it returns ProjectStatus as output If the Project does not exists projectstatus = null</param>
        /// <returns>Returns true if project exists in the database and not marked as deleted for that specific revision</returns>
        private bool TryGetProjectStatusIfProjectExist(InstanceItem project, out ProjectStatus? projectStatus)
        {
            if (project == null)
            {
                projectStatus = null;
                return false;
            }
            if (project.ParentFolderId == null)
            {
                projectStatus = null;
                return false;
            }

            projectStatus = GetProjectStatus(project.ProjectStatus);
            return true;
        }

        /// <summary>
        /// Maps the project status string to enum.
        /// </summary>
        private static ProjectStatus GetProjectStatus(string status)
        {
            // Project status is used to identify different status of the import process a project can be in
            switch (status)
            {
                case "I":
                    return ProjectStatus.Importing;
                case "F":
                    return ProjectStatus.ImportFailed;
                case "C":
                    return ProjectStatus.CancelingImport;
                case null:
                    return ProjectStatus.Live;
                default:
                    throw new Exception(I18NHelper.FormatInvariant(ErrorMessages.UnhandledStatusOfProject, status));
            }
        }

        #endregion
            }
}
