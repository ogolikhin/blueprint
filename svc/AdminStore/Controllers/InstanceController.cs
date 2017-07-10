using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Helpers;
using AdminStore.Models;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
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
        internal readonly IInstanceRepository _instanceRepository;
        internal readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        internal readonly PrivilegesManager _privilegesManager;

        public override string LogSource { get; } = "AdminStore.Instance";

        public InstanceController() : this(
            new SqlInstanceRepository(), new ServiceLogRepository(), 
            new SqlArtifactPermissionsRepository(), new SqlPrivilegesRepository()
        )
        {
        }

        public InstanceController
        (
            IInstanceRepository instanceRepository,
            IServiceLogRepository log,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IPrivilegesRepository privilegesRepository
        ) : base(log)
        {
            _instanceRepository = instanceRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
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
    }
}
