using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class AttachmentsController : LoggableApiController
    {
        private readonly IAttachmentsRepository AttachmentsRepository;
        private readonly IArtifactPermissionsRepository ArtifactPermissionsRepository;
        private readonly IArtifactVersionsRepository ArtifactVersionsRepository;

        public override string LogSource { get; } = "ArtifactStore.Attachments";

        public AttachmentsController() : this(new SqlAttachmentsRepository(),
            new SqlArtifactPermissionsRepository(),
            new SqlArtifactVersionsRepository())
        {
        }

        public AttachmentsController(IAttachmentsRepository attachmentsRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IArtifactVersionsRepository artifactVersionsRepository) : base()
        {
            AttachmentsRepository = attachmentsRepository;
            ArtifactPermissionsRepository = artifactPermissionsRepository;
            ArtifactVersionsRepository = artifactVersionsRepository;
        }

        /// <summary>
        /// Get artifact history
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="404">Not Found. The requested artifact or subartifact is deleted or does not exist.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/attachment"), SessionRequired]
        [ActionName("GetAttachmentsAndDocumentReferences")]
        public async Task<FilesInfo> GetAttachmentsAndDocumentReferences(
            int artifactId,
            int? versionId = null,
            int? subArtifactId = null,
            bool addDrafts = true,
            int? baselineId = null)
        {
            if (artifactId < 1 || (subArtifactId.HasValue && subArtifactId.Value < 1) || (versionId != null && baselineId != null))
            {
                throw new BadRequestException();
            }
            if (addDrafts && versionId != null)
            {
                addDrafts = false;
            }
            var userId = Session.UserId;
            var itemId = subArtifactId.HasValue ? subArtifactId.Value : artifactId;
            var isDeleted = await ArtifactVersionsRepository.IsItemDeleted(itemId);
            var itemInfo = isDeleted && (versionId != null || baselineId != null) ?
                (await ArtifactVersionsRepository.GetDeletedItemInfo(itemId)) :
                (await ArtifactPermissionsRepository.GetItemInfo(itemId, userId, addDrafts));
            if (itemInfo == null)
            {
                throw new ResourceNotFoundException("You have attempted to access an item that does not exist or you do not have permission to view.",
                    subArtifactId.HasValue ? ErrorCodes.SubartifactNotFound : ErrorCodes.ArtifactNotFound);
            }
            if (subArtifactId.HasValue && itemInfo.ArtifactId != artifactId)
            {
                throw new BadRequestException("Please provide a proper subartifact Id");
            }
            var result = await AttachmentsRepository.GetAttachmentsAndDocumentReferences(artifactId, userId, versionId, subArtifactId, addDrafts, baselineId);
            var artifactIds = new List<int> { artifactId };
            foreach (var documentReference in result.DocumentReferences)
            {
                artifactIds.Add(documentReference.ArtifactId);
            }
            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissions(artifactIds, userId);
            if (!SqlArtifactPermissionsRepository.HasPermissions(artifactId, permissions, RolePermissions.Read))
            {
                throw new AuthorizationException("You do not have permission to access the artifact");
            }
            var docRef = result.DocumentReferences.ToList();
            foreach (var documentReference in docRef)
            {
                if (!SqlArtifactPermissionsRepository.HasPermissions(documentReference.ArtifactId, permissions, RolePermissions.Read))
                {
                    result.DocumentReferences.Remove(documentReference);
                }
            }
            return result;
        }
    }
}
