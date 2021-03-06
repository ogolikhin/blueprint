﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ArtifactStore.Models.Review;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ArtifactController : LoggableApiController
    {
        private readonly IArtifactRepository _artifactRepository;

        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;

        private readonly IReviewsRepository _reviewsRepository;

        private readonly PrivilegesManager _privilegesManager;

        public override string LogSource { get; } = "ArtifactStore.Artifact";

        public ArtifactController() : this
            (
                new SqlArtifactRepository(),
                new SqlArtifactPermissionsRepository(),
                new SqlReviewsRepository(),
                new SqlPrivilegesRepository())
        {
        }

        public ArtifactController
        (
            IArtifactRepository instanceRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IReviewsRepository reviewsRepository,
            IPrivilegesRepository privilegesRepository)
        {
            _artifactRepository = instanceRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _reviewsRepository = reviewsRepository;
            _privilegesManager = new PrivilegesManager(privilegesRepository);
        }

        public ArtifactController
        (
            IArtifactRepository instanceRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IPrivilegesRepository privilegesRepository,
            IServiceLogRepository log) : base(log)
        {
            _artifactRepository = instanceRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
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
            return await _artifactRepository.GetProjectOrArtifactChildrenAsync(projectId, null, Session.UserId);
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
            return await _artifactRepository.GetProjectOrArtifactChildrenAsync(projectId, artifactId, Session.UserId);
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
            var userId = Session.UserId;
            var artifactIds = new[] { artifactId };
            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIds, userId, false);

            RolePermissions permission;
            if (!permissions.TryGetValue(artifactId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            return (await _artifactRepository.GetSubArtifactTreeAsync(artifactId, userId)).ToList();
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
            if (expandedToArtifactId < 1)
            {
                throw new BadRequestException($"Parameter {nameof(expandedToArtifactId)} must be greater than 0.", ErrorCodes.OutOfRangeParameter);
            }

            return await _artifactRepository.GetExpandedTreeToArtifactAsync(projectId, expandedToArtifactId, includeChildren, Session.UserId);
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
            return await _artifactRepository.GetArtifactNavigationPathAsync(artifactId, Session.UserId);
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
            return await _artifactRepository.GetAuthorHistoriesWithPermissionsCheck(artifactIds, Session.UserId);
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
        public async Task<IEnumerable<BaselineInfo>> GetBaselineInfo([FromBody] ISet<int> artifactIds, bool addDrafts = true)
        {
            return await _artifactRepository.GetBaselineInfo(artifactIds, Session.UserId, true, int.MaxValue);
        }

        /// <summary>
        /// Get the reviews information.
        /// </summary>
        /// <remarks>
        /// Returns for the each review expiry timestamp, review status and type
        /// If user doesn't have read permissions for all requested artifacts method returns empty IEnumerable.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("artifacts/reviewInfo"), SessionRequired]
        public async Task<IEnumerable<ReviewInfo>> GetReviewInfo([FromBody] ISet<int> artifactIds)
        {
            return await _reviewsRepository.GetReviewInfo(artifactIds, Session.UserId);
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
                throw new BadRequestException(ErrorMessages.ArtifactTypeIdsNotValid);
            }

            return await _artifactRepository.GetProcessInformationAsync(artifactIds, Session.UserId);
        }

        /// <summary>
        /// Get a list of all standard artifact types in the system.
        /// </summary>
        /// <remarks>
        /// Returns the list of standard artifact types. Every item of the list contains id and name of artifact.
        /// </remarks>
        /// <param name="filter">It is filter to receive only regular artifact types or all standard artifact types.</param>
        /// <response code="200">OK. The list of standard artifact types.</response>
        /// <response code="400">Incorrect filter parameter.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for geting the list of standard artifact types.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/standardartifacttypes"), SessionRequired]
        public async Task<IEnumerable<StandardArtifactType>> GetStandardArtifactTypes(StandardArtifactTypes filter = StandardArtifactTypes.All)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            if (filter != StandardArtifactTypes.All && filter != StandardArtifactTypes.Regular)
            {
                throw new BadRequestException(ErrorMessages.InvalidStandardArtifactTypesFilterValue);
            }

            return await _artifactRepository.GetStandardArtifactTypes(filter);
        }

        /// <summary>
        /// Get a list of standard properties for certain artifact types or all standard properties in system.
        /// </summary>
        /// <remarks>
        /// Return the list of standard properties.
        /// </remarks>
        /// <param name="standardArtifactTypeIds">It is filter to receive only standard properties for certain artifact types or all standard properties in system if the parameter doesn't have any ids.</param>
        /// <response code="200">OK. The list of standard properties.</response>
        /// <response code="400">Incorrect parameter.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for geting the list of standard properties.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route("artifacts/standardproperties"), SessionRequired]
        public async Task<IEnumerable<PropertyType>> GetStandardProperties([FromBody] ISet<int> standardArtifactTypeIds)
        {
            await _privilegesManager.Demand(Session.UserId, InstanceAdminPrivileges.AccessAllProjectData);

            if (standardArtifactTypeIds == null)
            {
                throw new BadRequestException(ErrorMessages.ModelIsEmpty);
            }

            return await _artifactRepository.GetStandardProperties(standardArtifactTypeIds);
        }
    }
}
