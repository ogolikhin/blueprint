﻿using System;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Models.DTO;
using AdminStore.Repositories;
using AdminStore.Services.Instance;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Models.Enums;
using ServiceLibrary.Helpers.Validators;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("instance")]
    [BaseExceptionFilter]
    public class InstanceController : LoggableApiController
    {
        private readonly IInstanceRepository _instanceRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IInstanceService _instanceService;
        private readonly PrivilegesManager _privilegesManager;
        private readonly IPrivilegesRepository _privilegesRepository;

        public override string LogSource { get; } = "AdminStore.Instance";

        public InstanceController() : this(
            new SqlInstanceRepository(), new ServiceLogRepository(),
            new SqlArtifactPermissionsRepository(), new SqlPrivilegesRepository(), new InstanceService())
        {
        }

        public InstanceController
        (
            IInstanceRepository instanceRepository,
            IServiceLogRepository log,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IPrivilegesRepository privilegesRepository,
            IInstanceService instanceService) : base(log)
        {
            _instanceRepository = instanceRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _instanceService = instanceService;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
            _privilegesRepository = privilegesRepository;
        }

        /// <summary>
        /// Get Instance Folder
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fromAdminPortal"></param>
        /// <remarks>
        /// Returns an instance folder for the specified id.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the folder.</response>
        /// <response code="404">Not found. An instance folder for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("folders/{id:int:min(1)}"), SessionRequired]
        [ResponseType(typeof(InstanceItem))]
        [ActionName("GetInstanceFolder")]
        public async Task<InstanceItem> GetInstanceFolderAsync(int id, bool fromAdminPortal = false)
        {
            if (fromAdminPortal)
            {
                await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ViewProjects);
            }

            return await _instanceRepository.GetInstanceFolderAsync(id, Session.UserId, fromAdminPortal);
        }

        /// <summary>
        /// Get Instance Folder Children
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fromAdminPortal"></param>
        /// <remarks>
        /// Returns child instance folders and live projects that the user has the read permission.
        /// If an instance folder for the specified id is not found, the empty collection is returned.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("folders/{id:int:min(1)}/children"), SessionRequired]
        [ResponseType(typeof(List<InstanceItem>))]
        [ActionName("GetInstanceFolderChildren")]
        public async Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int id, bool fromAdminPortal = false)
        {
            return await _instanceRepository.GetInstanceFolderChildrenAsync(id, Session.UserId, fromAdminPortal);
        }

        /// <summary>
        /// Search folder by name
        /// </summary>
        /// <param name="name">name of folder.</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("foldersearch"), SessionRequired]
        [ResponseType(typeof(IEnumerable<InstanceItem>))]
        public async Task<IHttpActionResult> SearchFolderByName(string name)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageProjects);
            var result = await _instanceService.GetFoldersByName(name);
            return Ok(result);
        }

        /// <summary>
        /// Get Project
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fromAdminPortal"></param>
        /// <remarks>
        /// Returns an instance folder for the specified id.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="404">Not found. A project for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("projects/{id:int:min(1)}"), SessionRequired]
        [ResponseType(typeof(InstanceItem))]
        [ActionName("GetInstanceProject")]
        public async Task<InstanceItem> GetInstanceProjectAsync(int id, bool fromAdminPortal = false)
        {
            return await _instanceRepository.GetInstanceProjectAsync(id, Session.UserId, fromAdminPortal);
        }

        /// <summary>
        /// Get Project Navigation Paths
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="includeProjectItself"></param>
        /// <returns> List of string for the navigation path of specified project id</returns>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="404">Not found. A project for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/navigationPath"), SessionRequired]
        [ActionName("GetProjectNavigationPath")]
        public async Task<List<string>> GetProjectNavigationPathAsync(int projectId, bool includeProjectItself = true)
        {
            var result = await _instanceRepository.GetProjectNavigationPathAsync(projectId, Session.UserId, includeProjectItself);

            var artifactIds = new[] { projectId };
            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIds, Session.UserId);

            RolePermissions permission;
            if (!permissions.TryGetValue(projectId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            return result;
        }

        /// <summary>
        /// Search projects and folders
        /// </summary>
        /// <param name="pagination">Limit and offset values to query results</param>
        /// <param name="sorting">(optional) Sort and its order</param>
        /// <param name="search">Search query parameter</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        [HttpGet, NoCache]
        [Route("folderprojectsearch"), SessionRequired]
        [ResponseType(typeof(QueryResult<ProjectFolderSearchDto>))]
        public async Task<IHttpActionResult> SearchProjectFolder([FromUri]Pagination pagination, [FromUri]Sorting sorting = null, string search = null)
        {
            pagination.Validate();
            SearchFieldValidator.Validate(search);

            var result =
                await
                    _instanceRepository.GetProjectsAndFolders(Session.UserId,
                        new TabularData() { Pagination = pagination, Sorting = sorting, Search = search },
                        SortingHelper.SortProjectFolders);
            return Ok(result);
        }

        #region folders

        /// <summary>
        /// Creation of instance folder.
        /// </summary>
        /// <remarks>
        /// Returns id of created folder.
        /// </remarks>
        /// <returns code="201">OK. Folder is created.</returns>
        /// <returns code="400">BadRequest. Parameters are invalid.</returns>
        /// <returns code="401">>Unauthorized. The session token is invalid, missing or malformed.</returns>
        /// <returns code="403">Forbidden. The user does not have permissions for creating the folder.</returns>
        /// <returns code="404">NotFound. The parent folder with current id does not exist.</returns>
        /// <returns code="409">Conflict. Folder with the same name already exists in the parent folder.</returns>
        /// <returns code="500">Internal server error.</returns>
        [HttpPost]
        [SessionRequired]
        [Route("folder")]
        [ResponseType(typeof(int))]
        public async Task<HttpResponseMessage> CreateFolder(FolderDto folder)
        {
            if (folder == null)
            {
                throw new BadRequestException(ErrorMessages.ModelIsEmpty, ErrorCodes.BadRequest);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageProjects);

            FolderValidator.ValidateModel(folder);

            var folderId = await _instanceRepository.CreateFolderAsync(folder);

            return Request.CreateResponse(HttpStatusCode.Created, folderId);
        }

        /// <summary>
        /// Delete Instance Folder
        /// </summary>
        /// <remarks>
        /// Returns count of deleted folders.
        /// </remarks>
        /// <response code="200">OK. An instance folder is deleted.</response>
        /// <response code="400">BadRequest. Some errors. </response>
        /// <response code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</response>
        /// <response code="403">Forbidden if used doesn’t have permissions to delete instance folder</response>
        /// <response code="404">NotFound. if instance folder with instanceFolderId doesn’t exists or removed from the system.</response>
        /// <response code="409">Conflict. The Folder cannot be deleted as it contains Projects and/or Folders.</response>
        [HttpDelete]
        [Route("folders/{instanceFolderId:int:min(1)}"), SessionRequired]
        [ResponseType(typeof(DeleteResult))]
        public async Task<IHttpActionResult> DeleteInstanceFolder(int instanceFolderId)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.DeleteProjects);

            var result = await _instanceRepository.DeleteInstanceFolderAsync(instanceFolderId);

            return Ok(new DeleteResult { TotalDeleted = result });
        }

        /// <summary>
        /// Update instance folder
        /// </summary>
        /// <param name="folderId">Instance folder id.</param>
        /// <param name="folderDto">Updated instance folder model.</param>
        /// <response code="204">NoContent. The instance folder is updated.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions to update the instance folder.</response>
        /// <response code="404">NotFound. The instance folder or its parent folder do not exist or are removed from the system.</response>
        /// <response code="409">Conflict. The instance folder with the same name already exists in the parent folder or the parent folder is invalid.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPut]
        [SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        [Route("folders/{folderId:int:min(1)}")]
        public async Task<HttpResponseMessage> UpdateInstanceFolder(int folderId, [FromBody] FolderDto folderDto)
        {
            if (folderDto == null)
            {
                throw new BadRequestException(ErrorMessages.ModelIsEmpty, ErrorCodes.BadRequest);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageProjects);

            FolderValidator.ValidateModel(folderDto, folderId);

            await _instanceRepository.UpdateFolderAsync(folderId, folderDto);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        #endregion

        #region projects

        /// <summary>
        /// Delete Project
        /// </summary>
        /// <remarks>
        /// Returns Ok result.
        /// </remarks>
        /// <response code="204">OK. The project is deleted.</response>
        /// <response code="400">BadRequest. Parameters are invalid.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden The user does not have permissions to delete project</response>
        /// <response code="404">NotFound. The project with ID:{0}({1}) doesn’t exists or removed from the system or was deleted by another user! </response>
        /// <response code="404">Could not purge project because an artifact was moved to another project and we cannot reliably purge it without corrupting the other project.  PurgeProject aborted for projectId  {0}.</response>
        /// <response code="409">Could not purge project because it is a system instance project for internal use only and without it database is corrupted. Purge project aborted for projectId {0}.</response>
        /// <response code="500">Internal Server Error.</response>
        [HttpDelete]
        [SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        [Route("projects/{projectId:int:min(1)}")]
        public async Task<HttpResponseMessage> DeleteProject(int projectId)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.DeleteProjects);

            await _instanceService.DeleteProject(Session.UserId, projectId);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Get Project privileges
        /// </summary>
        /// <param name="projectId"></param>
        /// <remarks>
        /// Returns privileges for the specified instance project id.
        /// </remarks>
        /// <response code="200">OK. The user's project privileges for the project is returned</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="404">Not found. The project with the current id does not exist.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/privileges"), SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetProjectAdminPermissions(int projectId)
        {
            var permissions = await _privilegesRepository.GetProjectAdminPermissionsAsync(Session.UserId, projectId);
            return Ok(permissions);
        }

        /// <summary>
        /// Check if project has external locks
        /// </summary>
        /// <param name="projectId"></param>
        /// <remarks>
        /// Returns boolean, if there are any external locks for the specified instance project id.
        /// </remarks>
        /// <response code="200">OK. Boolean, if there are any external locks for the specified instance project id.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden The user does not have permissions to check if project has external locks</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/hasprojectexternallocks"), SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> HasProjectExternalLocks(int projectId)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.DeleteProjects);

            var hasProjectExternalLocks = await _instanceRepository.HasProjectExternalLocksAsync(Session.UserId, projectId);
            return Ok(hasProjectExternalLocks);
        }

        #endregion

        #region roles

        /// <summary>
        /// Get the list of instance administrators roles in the instance
        /// </summary>
        /// <remarks>
        /// Returns the list of instance administrators roles.
        /// </remarks>
        /// <returns code="200">OK list of AdminRole models</returns>
        /// <returns code="400">BadRequest if errors occurred</returns>
        /// <returns code="401">Unauthorized if session token is missing, malformed or invalid (session expired)</returns>
        /// <returns code="403">Forbidden if used doesn’t have permissions to get the list of instance administrators roles</returns>
        [SessionRequired]
        [Route("roles")]
        [ResponseType(typeof(IEnumerable<AdminRole>))]
        public async Task<IHttpActionResult> GetInstanceRoles()
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ViewUsers);

            var result = await _instanceRepository.GetInstanceRolesAsync();

            return Ok(result);
        }

        /// <summary>
        /// Get roles for project
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <response code="200">OK. The roles for the project are returned</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. If used doesn’t have permissions to get project's roles.</response>
        /// <response code="404">NotFound. If roles with projectId don’t exists or removed from the system.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/roles"), SessionRequired]
        [ResponseType(typeof(QueryResult<ProjectRole>))]
        public async Task<IHttpActionResult> GetProjectRolesAsync(int projectId)
        {
            await _privilegesManager.DemandAny(Session.UserId, projectId, InstanceAdminPrivileges.AccessAllProjectsAdmin,
                    ProjectAdminPrivileges.ViewGroupsAndRoles);

            var result = await _instanceRepository.GetProjectRolesAsync(projectId);

            return Ok(result);
        }

        /// <summary>
        /// The method returns all role assignments for the specified project.
        /// </summary>
        /// <param name="projectId">Project's identity</param>
        /// <param name="pagination">Pagination parameters</param>
        /// <param name="sorting">Sorting parameters</param>
        /// <param name="search">The parameter for searching by group name.</param>
        /// <response code="200">OK. The list of role assignments for the project.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. if user doesn’t have permissions to get role assignments for the project.</response>
        /// <response code="404">NotFound. The project with the current id does not exist.</response>
        /// <response code="500">Internal Server Error.</response>
        [Route("projects/{projectId:int:min(1)}/rolesassignments")]
        [SessionRequired]
        [ResponseType(typeof(RoleAssignmentQueryResult<RoleAssignment>))]
        public async Task<IHttpActionResult> GetProjectRoleAssignments(int projectId, [FromUri]Pagination pagination, [FromUri]Sorting sorting, string search = null)
        {
            pagination.Validate();
            SearchFieldValidator.Validate(search);

            await
                _privilegesManager.DemandAny(Session.UserId, projectId, InstanceAdminPrivileges.AccessAllProjectsAdmin,
                    ProjectAdminPrivileges.ViewGroupsAndRoles);

            var tabularData = new TabularData { Pagination = pagination, Sorting = sorting, Search = search };
            var result = await _instanceRepository.GetProjectRoleAssignmentsAsync(projectId, tabularData, SortingHelper.SortProjectRolesAssignments);

            return Ok(result);
        }

        /// <summary>
        /// Delete role assignment/assignments
        /// </summary>
        /// <param name="projectId">Project's identity</param>
        /// <param name="scope">list of role assignment ids and selectAll flag</param>
        /// <param name="search">The parameter for searching by group name.</param>
        /// <remarks>
        /// Returns Ok result.
        /// </remarks>
        /// <response code="200">OK. Role assignments were deleted.</response>
        /// <response code="400">BadRequest. Parameters are invalid.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden The user does not have permissions to delete assignment/assignments</response>
        /// <response code="404">NotFound. The project with the current id doesn't exist or removed from the system.</response>
        /// <response code="500">Internal Server Error.</response>
        [HttpPost]
        [SessionRequired]
        [ResponseType(typeof(DeleteResult))]
        [Route("projects/{projectId:int:min(1)}/rolesassignments/delete")]
        public async Task<IHttpActionResult> DeleteRoleAssignment(int projectId, [FromBody] OperationScope scope, string search = null)
        {
            SearchFieldValidator.Validate(search);

            if (scope == null)
            {
                throw new BadRequestException(ErrorMessages.InvalidDeleteRoleAssignmentsParameters, ErrorCodes.BadRequest);
            }

            if (scope.IsEmpty())
            {
                return Ok(DeleteResult.Empty);
            }

            await _privilegesManager.DemandAny(Session.UserId, projectId,
                InstanceAdminPrivileges.AccessAllProjectsAdmin, ProjectAdminPrivileges.ManageGroupsAndRoles);

            var result = await _instanceRepository.DeleteRoleAssignmentsAsync(projectId, scope, search);

            return Ok(new DeleteResult { TotalDeleted = result });
        }



        /// <summary>
        /// Create role assignment
        /// </summary>
        /// <param name="projectId">Project's identity</param>
        /// <param name="roleAssignment">Role assignment model</param>
        /// <remarks>
        /// Returns id of creted role assignment.
        /// </remarks>
        /// <response code="200">OK. Role assignment was created.</response>
        /// <response code="400">BadRequest. Parameters are invalid.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden The user does not have permissions to create role assignment</response>
        /// <response code="404">NotFound. The project with the current id doesn't exist or removed from the system or
        /// the group with the current id is not found on the instance and project levels or
        /// the role with the current id is not found in the project's roles.</response>
        /// <response code="409">Conflict. The project role assignment with same data already exists.</response>
        /// <response code="500">Internal Server Error.</response>
        [HttpPost]
        [SessionRequired]
        [ResponseType(typeof(int))]
        [Route("projects/{projectId:int:min(1)}/rolesassignments")]
        public async Task<HttpResponseMessage> CreateRoleAssignment(int projectId, [FromBody] RoleAssignmentDTO roleAssignment)
        {
            if (roleAssignment == null)
            {
                throw new BadRequestException(ErrorMessages.ModelIsEmpty, ErrorCodes.BadRequest);
            }

            await _privilegesManager.DemandAny(Session.UserId, projectId,
                InstanceAdminPrivileges.AccessAllProjectsAdmin, ProjectAdminPrivileges.ManageGroupsAndRoles);

            RoleAssignmentValidator.ValidateModel(roleAssignment);

            var createdRoleAssignmentId = await _instanceRepository.CreateRoleAssignmentAsync(projectId, roleAssignment);

            return Request.CreateResponse(HttpStatusCode.Created, createdRoleAssignmentId);
        }

        /// <summary>
        /// Update role assignment
        /// </summary>
        /// <param name="projectId">Project's identity</param>
        /// <param name="roleAssignment">Role assignment model</param>
        /// <param name="roleAssignmentId">Role assignment id</param>
        /// <remarks>
        /// Returns id of updated role assignment (newly created).
        /// </remarks>
        /// <response code="204">OK. Role assignment was updated.</response>
        /// <response code="400">BadRequest. Parameters are invalid.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden The user does not have permissions to update role assignment</response>
        /// <response code="404">NotFound. The project with the current id doesn't exist or removed from the system or
        /// the group with the current id is not found on the instance and project levels or
        /// the role with the current id is not found in the project's roles or role assignment with the current id is not found </response>
        /// <response code="409">Conflict. Combination of current project, group and role already exists.</response>
        /// <response code="500">Internal Server Error.</response>
        [HttpPut]
        [SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        [Route("projects/{projectId:int:min(1)}/rolesassignments/{roleAssignmentId:int:min(1)}")]
        public async Task<HttpResponseMessage> UpdateRoleAssignment(int projectId, int roleAssignmentId, [FromBody] RoleAssignmentDTO roleAssignment)
        {
            if (roleAssignment == null)
            {
                throw new BadRequestException(ErrorMessages.ModelIsEmpty, ErrorCodes.BadRequest);
            }

            await _privilegesManager.DemandAny(Session.UserId, projectId,
                InstanceAdminPrivileges.AccessAllProjectsAdmin, ProjectAdminPrivileges.ManageGroupsAndRoles);

            RoleAssignmentValidator.ValidateModel(roleAssignment, OperationMode.Edit, roleAssignmentId);

            await _instanceRepository.UpdateRoleAssignmentAsync(projectId, roleAssignmentId, roleAssignment);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        #endregion
    }
}
