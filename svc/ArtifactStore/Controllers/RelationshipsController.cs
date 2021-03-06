﻿using ArtifactStore.Models;
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
    public class RelationshipsController : LoggableApiController
    {
        private readonly IRelationshipsRepository _relationshipsRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;

        public override string LogSource { get; } = "ArtifactStore.Relationships";

        public RelationshipsController() : this(new SqlRelationshipsRepository(), new SqlArtifactPermissionsRepository(), new SqlArtifactVersionsRepository())
        {
        }

        public RelationshipsController(IRelationshipsRepository relationshipsRepository,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            IArtifactVersionsRepository artifactVersionsRepository) : base()
        {
            _relationshipsRepository = relationshipsRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _artifactVersionsRepository = artifactVersionsRepository;
        }

        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/relationships"), SessionRequired]
        [ActionName("GetRelationships")]
        public async Task<RelationshipResultSet> GetRelationships(
            int artifactId,
            int? subArtifactId = null,
            bool addDrafts = true,
            int? versionId = null,
            int? baselineId = null,
            bool? allLinks = null)
        {
            var userId = Session.UserId;
            if (artifactId < 1 || (subArtifactId.HasValue && subArtifactId.Value < 1)
                || versionId.HasValue && versionId.Value < 1 || (versionId != null && baselineId != null))
            {
                throw new BadRequestException();
            }
            if (addDrafts && versionId != null)
            {
                addDrafts = false;
            }
            var itemId = subArtifactId ?? artifactId;
            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(itemId);
            var itemInfo = isDeleted && (versionId != null || baselineId != null) ?
                await _artifactVersionsRepository.GetDeletedItemInfo(itemId) :
                await _artifactPermissionsRepository.GetItemInfo(itemId, userId, addDrafts);
            if (itemInfo == null)
            {
                throw new ResourceNotFoundException("You have attempted to access an item that does not exist or you do not have permission to view.",
                    subArtifactId.HasValue ? ErrorCodes.SubartifactNotFound : ErrorCodes.ArtifactNotFound);
            }

            if (subArtifactId.HasValue && itemInfo.ArtifactId != artifactId)
            {
                throw new BadRequestException("Please provide a proper subartifact Id");
            }

            // We do not need drafts for historical artifacts
            var effectiveAddDraft = !versionId.HasValue && addDrafts;

            var result = await _relationshipsRepository.GetRelationships(artifactId, userId, subArtifactId, effectiveAddDraft, allLinks.GetValueOrDefault(), versionId, baselineId);
            var artifactIds = new List<int> { artifactId };
            artifactIds = artifactIds.Union(result.ManualTraces.Select(a => a.ArtifactId)).Union(result.OtherTraces.Select(a => a.ArtifactId)).Distinct().ToList();
            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIds, userId);
            if (!SqlArtifactPermissionsRepository.HasPermissions(artifactId, permissions, RolePermissions.Read))
            {
                throw new AuthorizationException("You do not have permission to access the artifact");
            }

            ApplyRelationshipPermissions(permissions, result.ManualTraces);
            ApplyRelationshipPermissions(permissions, result.OtherTraces);

            result.CanEdit = SqlArtifactPermissionsRepository.HasPermissions(artifactId, permissions, RolePermissions.Trace) && SqlArtifactPermissionsRepository.HasPermissions(artifactId, permissions, RolePermissions.Edit);

            return result;
        }

        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/relationshipdetails"), SessionRequired]
        [ActionName("GetRelationshipDetails")]
        public async Task<RelationshipExtendedInfo> GetRelationshipDetails(int artifactId, int? subArtifactId = null)
        {
            var userId = Session.UserId;
            if (artifactId < 1)
            {
                throw new BadRequestException();
            }

            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(artifactId);
            var artifactInfo = isDeleted ?
                await _artifactVersionsRepository.GetDeletedItemInfo(artifactId) :
                await _artifactPermissionsRepository.GetItemInfo(artifactId, userId);

            if (artifactInfo == null && !isDeleted) // artifact might have been deleted in draft only
            {
                artifactInfo = await _artifactPermissionsRepository.GetItemInfo(artifactId, userId, false);
            }
            if (artifactInfo == null)
            {
                throw new ResourceNotFoundException("You have attempted to access an item that does not exist or you do not have permission to view.",
                    ErrorCodes.ArtifactNotFound);
            }

            var itemIds = new List<int> { artifactId };
            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(itemIds, userId);
            if (!SqlArtifactPermissionsRepository.HasPermissions(artifactId, permissions, RolePermissions.Read))
            {
                throw new AuthorizationException("You do not have permission to access the artifact");
            }
            return await _relationshipsRepository.GetRelationshipExtendedInfo(artifactId, userId, subArtifactId, isDeleted);
        }


        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/reviews"), SessionRequired]
        [ActionName("GetReviews")]
        public async Task<ReviewRelationshipsResultSet> GetReviewRelationships(int artifactId, bool addDrafts = true, int? versionId = null)
        {
            var userId = Session.UserId;
            if (artifactId < 1 || versionId.HasValue && versionId.Value < 1)
            {
                throw new BadRequestException();
            }
            if (addDrafts && versionId != null)
            {
                addDrafts = false;
            }

            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(artifactId);
            var itemInfo = isDeleted && versionId != null ?
                await _artifactVersionsRepository.GetDeletedItemInfo(artifactId) :
                await _artifactPermissionsRepository.GetItemInfo(artifactId, userId, addDrafts);

            if (itemInfo == null)
            {
                throw new ResourceNotFoundException();
            }

            // We do not need drafts for historical artifacts
            var effectiveAddDraft = !versionId.HasValue && addDrafts;
            var result = await _relationshipsRepository.GetReviewRelationships(artifactId, userId, effectiveAddDraft, versionId);
            var artifactIds = new List<int> { artifactId };
            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(artifactIds, userId);
            if (!SqlArtifactPermissionsRepository.HasPermissions(artifactId, permissions, RolePermissions.Read))
            {
                throw new AuthorizationException("You do not have permission to access the artifact");
            }
            return result;
        }

        private void ApplyRelationshipPermissions(Dictionary<int, RolePermissions> permissions, List<Relationship> relationships)
        {
            foreach (var relationship in relationships)
            {
                if (!SqlArtifactPermissionsRepository.HasPermissions(relationship.ArtifactId, permissions, RolePermissions.Read))
                {
                    MakeRelationshipUnauthorized(relationship);
                }

                if ((SqlArtifactPermissionsRepository.HasPermissions(relationship.ArtifactId, permissions, RolePermissions.Trace) &&
                     SqlArtifactPermissionsRepository.HasPermissions(relationship.ArtifactId, permissions, RolePermissions.Edit)) == false)
                {
                    relationship.ReadOnly = true;
                }
            }
        }

        private static void MakeRelationshipUnauthorized(Relationship relationship)
        {
            relationship.HasAccess = false;
            relationship.ArtifactName = null;
            relationship.ItemLabel = null;
            relationship.ItemName = null;
        }
    }
}
