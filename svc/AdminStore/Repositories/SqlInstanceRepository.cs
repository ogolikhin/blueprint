using AdminStore.Models;
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
using AdminStore.Helpers;
using AdminStore.Models.DTO;
using AdminStore.Models.Enums;

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

        public async Task<InstanceItem> GetInstanceFolderAsync(int folderId, int userId)
        {
            if (folderId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(folderId));
            }

            var prm = new DynamicParameters();
            prm.Add("@folderId", folderId);
            prm.Add("@userId", userId);

            var folder = (await _connectionWrapper.QueryAsync<InstanceItem>("GetInstanceFolderById", prm, commandType: CommandType.StoredProcedure))?.FirstOrDefault();
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

        public async Task<IEnumerable<AdminRole>> GetInstanceRolesAsync()
        {
            var result = await _connectionWrapper.QueryAsync<AdminRole>("GetInstanceAdminRoles", commandType: CommandType.StoredProcedure);

            return result;
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

                    case (int) SqlErrorCodes.FolderWithSuchNameExistsInParentFolder:
                        throw new ConflictException(ErrorMessages.FolderWithSuchNameExistsInParentFolder, ErrorCodes.Conflict);

                    case (int)SqlErrorCodes.ParentFolderNotExists:
                        throw new ResourceNotFoundException(ErrorMessages.ParentFolderNotExists, ErrorCodes.ResourceNotFound);

                    default:
                        return folderId;
                }
            }
            return folderId;
        }

        public async Task<IEnumerable<FolderDto>> GetFoldersByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = UsersHelper.ReplaceWildcardCharacters(name);
            }
            var parameters = new DynamicParameters();
            parameters.Add("@name", name);

            var result =
                await
                    _connectionWrapper.QueryAsync<FolderDto>("GetFoldersByName", parameters,
                        commandType: CommandType.StoredProcedure);
            return result;
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

        public async Task UpdateProjectAsync(int projectId, ProjectDto projectDto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@newProjectName", projectDto.Name);
            parameters.Add("@newProjectDescription", projectDto.Description);
            parameters.Add("@projectId", projectId);
            parameters.Add("@newParentFolderId", projectDto.ParentFolderId);

            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _connectionWrapper.ExecuteScalarAsync<int>("UpdateProject", parameters, commandType: CommandType.StoredProcedure);
            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue)
            {
                switch (errorCode.Value)
                {
                    case (int)SqlErrorCodes.GeneralSqlError:
                        throw new Exception(ErrorMessages.GeneralErrorOfUpdatingProject);

                    case (int)SqlErrorCodes.ProjectWithCurrentIdNotExist:
                        throw new ResourceNotFoundException(ErrorMessages.ProjectNotExist, ErrorCodes.ResourceNotFound);

                    case (int)SqlErrorCodes.ProjectWithSuchNameExistsInParentFolder:
                        throw new ConflictException(ErrorMessages.ProjectWithSuchNameExistsInParentFolder, ErrorCodes.Conflict);

                    case (int)SqlErrorCodes.ParentFolderNotExists:
                        throw new ResourceNotFoundException(ErrorMessages.ParentFolderNotExists, ErrorCodes.ResourceNotFound);
                }
            }
        }

        public async Task DeleteProject(int userId, int projectId)
        {
            //We need to check if project is still exist in database and not makred as deleted
            //Also we need to get the latest projectstatus to apply the right delete method
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
                            throw new BadRequestException(I18NHelper.FormatInvariant(ErrorMessages.ForbidToPurgeSystemInstanceProjectForInternalUseOnly, project.Id), ErrorCodes.BadRequest);
                        case -1: // Cross project move issue
                            throw new BadRequestException(I18NHelper.FormatInvariant(ErrorMessages.ArtifactWasMovedToAnotherProject, project.Id), ErrorCodes.BadRequest);
                        case 0:
                            // Success
                            break;
                        default:
                            throw new Exception(ErrorMessages.GeneralErrorOfUpdatingProject);
                    }
                }
            }
        }


        public async Task<QueryResult<RolesAssignments>> GetProjectRoleAssignmentsAsync(int projectId, TabularData tabularData, Func<Sorting, string> sort = null)
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
            parameters.Add("@ErrorCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var rolesAssigments = await _connectionWrapper.QueryAsync<RolesAssignments>("GetProjectRoleAssignments", parameters, commandType: CommandType.StoredProcedure);

            var errorCode = parameters.Get<int?>("ErrorCode");

            if (errorCode.HasValue && errorCode.Value == (int)SqlErrorCodes.ProjectWithCurrentIdNotExist)
            {
                throw new ResourceNotFoundException(ErrorMessages.ProjectNotExist, ErrorCodes.ResourceNotFound);
            }

            var total = parameters.Get<int?>("Total");

            var queryDataResult = new QueryResult<RolesAssignments> { Items = rolesAssigments, Total = total ?? 0 };

            return queryDataResult;
        }

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
    }
}
