﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Exceptions;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class AttachmentsController : LoggableApiController
    {
        internal readonly IAttachmentsRepository AttachmentsRepository;
        internal readonly IArtifactPermissionsRepository ArtifactPermissionsRepository;
        internal readonly IArtifactVersionsRepository ArtifactVersionsRepository;

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
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            if (addDrafts && versionId != null)
            {
                addDrafts = false;
            }
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            var itemId = subArtifactId.HasValue ? subArtifactId.Value : artifactId;
            var isDeleted = await ArtifactVersionsRepository.IsItemDeleted(itemId);
            var itemInfo = isDeleted && (versionId != null || baselineId != null) ?
                (await ArtifactVersionsRepository.GetDeletedItemInfo(itemId)) :
                (await ArtifactPermissionsRepository.GetItemInfo(itemId, session.UserId, addDrafts));
            if (itemInfo == null)
            {
                throw new ResourceNotFoundException("You have attempted to access an item that does not exist or you do not have permission to view.",
                    subArtifactId.HasValue ? ErrorCodes.SubartifactNotFound : ErrorCodes.ArtifactNotFound);
            }
            if (subArtifactId.HasValue)
            {
                if (itemInfo.ArtifactId != artifactId)
                {
                    throw new BadRequestException();
                }
            }
            var result = await AttachmentsRepository.GetAttachmentsAndDocumentReferences(artifactId, session.UserId, versionId, subArtifactId, addDrafts, baselineId);
            var artifactIds = new List<int> { artifactId };
            foreach (var documentReference in result.DocumentReferences)
            {
                artifactIds.Add(documentReference.ArtifactId);
            }
            var permissions = await ArtifactPermissionsRepository.GetArtifactPermissions(artifactIds, session.UserId);
            if(!SqlArtifactPermissionsRepository.HasPermissions(artifactId, permissions, RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
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
