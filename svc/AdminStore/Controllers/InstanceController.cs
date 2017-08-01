using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
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

        public override string LogSource { get; } = "AdminStore.Instance";

        public InstanceController() : this(
            new SqlInstanceRepository(), new ServiceLogRepository(),
            new SqlArtifactPermissionsRepository(), new SqlPrivilegesRepository(), new InstanceService()
        )
        {
        }

        public InstanceController
        (
            IInstanceRepository instanceRepository,
            IServiceLogRepository log,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IPrivilegesRepository privilegesRepository,
            IInstanceService instanceService
        ) : base(log)
        {
            _instanceRepository = instanceRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _instanceService = instanceService;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        /// <summary>
        /// Get Instance Folder
        /// </summary>
        /// <remarks>
        /// Returns an instance folder for the specified id.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="404">Not found. An instance folder for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("folders/{id:int:min(1)}"), SessionRequired]
        [ResponseType(typeof(InstanceItem))]
        [ActionName("GetInstanceFolder")]
        public async Task<InstanceItem> GetInstanceFolderAsync(int id)
        {
            return await _instanceRepository.GetInstanceFolderAsync(id, Session.UserId);
        }

        /// <summary>
        /// Get Instance Folder Children
        /// </summary>
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
        public async Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int id)
        {
            return await _instanceRepository.GetInstanceFolderChildrenAsync(id, Session.UserId);
        }

        /// <summary>
        /// Search folder by name
        /// </summary>
        /// <param name="name">name of folder.</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet,NoCache]
        [Route("foldersearch"), SessionRequired]
        [ResponseType(typeof(IEnumerable<FolderDto>))]
        public async Task<IHttpActionResult> SearchFolderByName(string name)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageProjects);
            var result = await _instanceService.GetFoldersByName(name);
            return Ok(result);
        }

        /// <summary>
        /// Get Project
        /// </summary>
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
        public async Task<InstanceItem> GetInstanceProjectAsync(int id)
        {
            return await _instanceRepository.GetInstanceProjectAsync(id, Session.UserId);
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
                throw new BadRequestException(ErrorMessages.FolderModelIsEmpty, ErrorCodes.BadRequest);
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
        [HttpDelete]
        [Route("folder/{instanceFolderId:int:min(1)}"), SessionRequired]
        [ResponseType(typeof(DeleteResult))]
        public async Task<IHttpActionResult> DeleteInstanceFolder(int instanceFolderId)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.DeleteProjects);

            var result = await _instanceRepository.DeleteInstanceFolderAsync(instanceFolderId);

            return Ok(new DeleteResult { TotalDeleted = result });
        }

        /// <summary>
        /// Update folder
        /// </summary>
        /// <param name="folderId">Folder's identity</param>
        /// <param name="folderDto">Folder's model</param>
        /// <remarks>
        /// Returns Ok result.
        /// </remarks>
        /// <response code="200">OK. The folder is updated.</response>
        /// <response code="400">BadRequest. Parameters are invalid. </response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for updating the folder.</response>
        /// <response code="404">NotFound. The folder with the current folderId doesn’t exist or removed from the system.</response>
        [HttpPut]
        [SessionRequired]
        [ResponseType(typeof(HttpResponseMessage))]
        [Route("folder/{folderId:int:min(1)}")]
        public async Task<IHttpActionResult> UpdateInstanceFolder(int folderId, [FromBody] FolderDto folderDto)
        {
            if (folderDto == null)
            {
                throw new BadRequestException(ErrorMessages.FolderModelIsEmpty, ErrorCodes.BadRequest);
            }

            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.ManageProjects);

            FolderValidator.ValidateModel(folderDto);
            
            await _instanceRepository.UpdateFolderAsync(folderId, folderDto);

            return Ok();
        }

        #endregion
    }
}
