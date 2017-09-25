using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls.Expressions;
using AdminStore.Helpers;
using AdminStore.Repositories;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ArtifactController : LoggableApiController
    {
        private readonly ISqlArtifactRepository ArtifactRepository;
        private readonly IArtifactPermissionsRepository ArtifactPermissionsRepository;
        private readonly PrivilegesManager _privilegesManager;

        public override string LogSource { get; } = "ArtifactStore.Artifact";

        public ArtifactController() : this(new SqlArtifactRepository(), new SqlArtifactPermissionsRepository(), new SqlPrivilegesRepository())
        {
        }

        public ArtifactController(ISqlArtifactRepository instanceRepository, IArtifactPermissionsRepository artifactPermissionsRepository, IPrivilegesRepository privilegesRepository) : base()
        {
            ArtifactRepository = instanceRepository;
            ArtifactPermissionsRepository = artifactPermissionsRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        public ArtifactController(ISqlArtifactRepository instanceRepository, IArtifactPermissionsRepository artifactPermissionsRepository, IPrivilegesRepository privilegesRepository, IServiceLogRepository log) : base(log)
        {
            ArtifactRepository = instanceRepository;
            ArtifactPermissionsRepository = artifactPermissionsRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        /// <summary>
        /// Get child artifacts of the project.
        /// </summary>
        /// <remarks>
        /// Returns child artifacts of the project with the specified id.
        /// </remarks>
        /// <param name="projectId">Id of the project</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="404">Not found. A project for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/children"), SessionRequired]
        [ActionName("GetProjectChildren")]
        public async Task<List<Artifact>> GetProjectChildrenAsync(int projectId)
        {
            return await ArtifactRepository.GetProjectOrArtifactChildrenAsync(projectId, null, Session.UserId);
        }

        /// <summary>
        /// Get child artifacts of the artifact.
        /// </summary>
        /// <remarks>
        /// Returns child artifacts of the artifact with the specified project and artifact ids.
        /// </remarks>
        /// <param name="projectId">Id of the project</param>
        /// <param name="artifactId">Id of the artifact</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the artifact.</response>
        /// <response code="404">Not found. A project or an artifact for the specified ids is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/artifacts/{artifactId:int:min(1)}/children"), SessionRequired]
        [ActionName("GetArtifactChildren")]
        public async Task<List<Artifact>> GetArtifactChildrenAsync(int projectId, int artifactId)
        {
            return await ArtifactRepository.GetProjectOrArtifactChildrenAsync(projectId, artifactId, Session.UserId);
        }

        /// <summary>
        /// Get sub artifact tree of the artifact.
        /// </summary>
        /// <remarks>
        /// Returns a constructed tree node representation of a given artifact's subartifacts.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the artifact.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/subartifacts"), SessionRequired]
        [ActionName("GetSubArtifactTreeAsync")]
        public async Task<List<SubArtifact>> GetSubArtifactTreeAsync(int artifactId)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            var artifactIds = new[] { artifactId };
            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissions(artifactIds, session.UserId, false);

            RolePermissions permission = RolePermissions.None;
            if (!permissions.TryGetValue(artifactId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            return (await ArtifactRepository.GetSubArtifactTreeAsync(artifactId, session.UserId)).ToList();
        }

        /// <summary>
        /// Get the artifact tree expended to the artifact.
        /// </summary>
        /// <remarks>
        /// Returns the tree of artifacts expended to the artifact with the specified project and artifact ids.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the artifact.</response>
        /// <response code="404">Not found. A project or an artifact for the specified ids is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("projects/{projectId:int:min(1)}/artifacts/{expandedToArtifactId=expandedToArtifactId}/{includeChildren=includeChildren?}"), SessionRequired]
        [ActionName("GetExpandedTreeToArtifact")]
        public async Task<List<Artifact>> GetExpandedTreeToArtifactAsync(int projectId, int expandedToArtifactId, bool includeChildren = false)
        {
            if(expandedToArtifactId < 1)
            {
                throw new BadRequestException(string.Format("Parameter {0} must be greater than 0.", nameof(expandedToArtifactId)), ErrorCodes.OutOfRangeParameter);
            }

            return await ArtifactRepository.GetExpandedTreeToArtifactAsync(projectId, expandedToArtifactId, includeChildren, Session.UserId);
        }

        /// <summary>
        /// Get the artifact navigation path, basic information of the artifact ancestors including the project.
        /// </summary>
        /// <remarks>
        /// Returns the artifact navigation path, basic information of the artifact ancestors including the project.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the artifact.</response>
        /// <response code="404">Not found. A artifact for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/navigationPath"), SessionRequired]
        [ActionName("GetArtifactNavigationPath")]
        public async Task<List<Artifact>> GetArtifactNavigationPathAsync(int artifactId)
        {
            return await ArtifactRepository.GetArtifactNavigationPathAsync(artifactId, Session.UserId);
        }

        /// <summary>
        /// Get the artifact author history information.
        /// </summary>
        /// <remarks>
        /// Returns for the each artifact created and last edited information with permissions check.
        /// If user doesn't have read permissions for all requested artifacts method returns empty IEnumerable.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>              
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("artifacts/authorHistories"), SessionRequired]        
        public async Task<IEnumerable<AuthorHistory>> GetArtifactsAuthorHistories([FromBody] ISet<int> artifactIds)
        {
            return await ArtifactRepository.GetAuthorHistoriesWithPermissionsCheck(artifactIds, Session.UserId);
        }

        /// <summary>
        /// Get the baselines information.
        /// </summary>
        /// <remarks>
        /// Returns for the each baseline IsSealed and Timestamp properties.
        /// If user doesn't have read permissions for all requested artifacts method returns empty IEnumerable.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>              
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("artifacts/baselineInfo"), SessionRequired]
        public async Task<IEnumerable<BaselineInfo>> GetBaselineInfo([FromBody] ISet<int> artifactIds)
        {
            return await ArtifactRepository.GetBaselineInfo(artifactIds, Session.UserId, true, int.MaxValue);
        }

        /// <summary>
        /// Get process information.
        /// </summary>
        /// <remarks>
        /// Returns list of objects with information about Process type for artifacts passed as parameters
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>              
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("artifacts/processInfo"), SessionRequired]
        public async Task<IEnumerable<ProcessInfoDto>> GetProcessInformationAsync([FromBody] ISet<int> artifactIds)
        {
            if (artifactIds == null)
            {
                throw new BadRequestException(ErrorMessages.ArtifactIdsNotValid);
            }

            return await ArtifactRepository.GetProcessInformationAsync(artifactIds);
        }

        /// <summary>
        /// Get a list of all standard artifact types in the system.
        /// </summary>
        /// <remarks>
        /// Returns the list of standard artifact types. Every item of the list contains id and name of artifact. 
        /// </remarks>
        /// <response code="200">OK. The list of standard artifact types.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>   
        /// <response code="403">Forbidden. The user does not have permissions for geting the list of standard artifact types.</response>           
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/standardartifacttypes"), SessionRequired]
        public async Task<IEnumerable<StandardArtifactType>> GetStandardArtifactTypes()
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            var artifacts = await ArtifactRepository.GetStandardArtifactTypes();
            return artifacts;
        }
    }
}
