using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class DiscussionController : LoggableApiController
    {
        private readonly IDiscussionsRepository _discussionsRepository;
        private readonly IArtifactPermissionsRepository _artifactPermissionsRepository;
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;

        public override string LogSource { get; } = "ArtifactStore.ItemDiscussions";

        public DiscussionController() : this(new SqlDiscussionsRepository(), new SqlArtifactPermissionsRepository(), new SqlArtifactVersionsRepository())
        {
        }

        public DiscussionController(IDiscussionsRepository discussionsRepository,
                                    IArtifactPermissionsRepository artifactPermissionsRepository,
                                    IArtifactVersionsRepository artifactVersionsRepository) : base()
        {
            _discussionsRepository = discussionsRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
            _artifactVersionsRepository = artifactVersionsRepository;
        }

        /// <summary>
        /// Get item discussions
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/discussions"), SessionRequired]
        [ActionName("GetDiscussions")]
        public async Task<DiscussionResultSet> GetDiscussions(int artifactId, int? subArtifactId = null)
        {
            ValidateRequestParameters(artifactId, subArtifactId);
            var userId = Session.UserId;

            var itemId = subArtifactId.HasValue ? subArtifactId.Value : artifactId;
            var revisionId = int.MaxValue;
            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(itemId);
            var itemInfo = isDeleted ?
                await _artifactVersionsRepository.GetDeletedItemInfo(itemId) :
                await _artifactPermissionsRepository.GetItemInfo(itemId, userId, false);
            if (itemInfo == null)
            {
                throw new ResourceNotFoundException("You have attempted to access an item that does not exist or you do not have permission to view.",
                    subArtifactId.HasValue ? ErrorCodes.SubartifactNotFound : ErrorCodes.ArtifactNotFound);
            }
            if (subArtifactId.HasValue && itemInfo.ArtifactId != artifactId)
            {
                throw new BadRequestException("Please provide a proper subartifact Id");
            }
            if (isDeleted)
            {
                revisionId = ((DeletedItemInfo)itemInfo).VersionId;
            }

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(new[] { artifactId }, userId, false, revisionId);
            var projectPermissions = await _artifactPermissionsRepository.GetProjectPermissions(itemInfo.ProjectId);

            RolePermissions permission = RolePermissions.None;
            if (!permissions.TryGetValue(artifactId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new AuthorizationException("You do not have permission to access the artifact");
            }

            var discussions = await _discussionsRepository.GetDiscussions(itemId, itemInfo.ProjectId);

            foreach (var discussion in discussions)
            {
                discussion.CanDelete = !projectPermissions.HasFlag(ProjectPermissions.CommentsDeletionDisabled)
                          && permissions.TryGetValue(artifactId, out permission) &&
                    (permission.HasFlag(RolePermissions.DeleteAnyComment) || (permission.HasFlag(RolePermissions.Comment) && discussion.UserId == userId));
                discussion.CanEdit = !projectPermissions.HasFlag(ProjectPermissions.CommentsModificationDisabled)
                          && permissions.TryGetValue(artifactId, out permission) && (permission.HasFlag(RolePermissions.Comment) && discussion.UserId == userId);
            }

            var availableStatuses = await _discussionsRepository.GetThreadStatusCollection(itemInfo.ProjectId);

            var result = new DiscussionResultSet
            {
                CanDelete = !projectPermissions.HasFlag(ProjectPermissions.CommentsDeletionDisabled)
                          && permission.HasFlag(RolePermissions.DeleteAnyComment) && revisionId == int.MaxValue,
                CanCreate = permission.HasFlag(RolePermissions.Comment) && revisionId == int.MaxValue,
                Discussions = discussions,
                EmailDiscussionsEnabled = await _discussionsRepository.AreEmailDiscussionsEnabled(itemInfo.ProjectId),
                ThreadStatuses = availableStatuses
            };
            return result;
        }

        /// <summary>
        /// Get item discussions
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("artifacts/{artifactId:int:min(1)}/discussions/{discussionId:int:min(1)}/replies"), SessionRequired]
        [ActionName("GetReplies")]
        public async Task<IEnumerable<Reply>> GetReplies(int artifactId, int discussionId, int? subArtifactId = null)
        {
            ValidateRequestParameters(artifactId, subArtifactId);

            if (discussionId < 1)
            {
                throw new BadRequestException(I18NHelper.FormatInvariant("Parameter: {0} is out of the range of valid values", nameof(discussionId)));
            }

            var userId = Session.UserId;

            var itemId = subArtifactId.HasValue ? subArtifactId.Value : artifactId;
            var revisionId = int.MaxValue;
            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(itemId);
            var itemInfo = isDeleted ?
                await _artifactVersionsRepository.GetDeletedItemInfo(itemId) :
                await _artifactPermissionsRepository.GetItemInfo(itemId, userId, false);

            if (itemInfo == null || await _discussionsRepository.IsDiscussionDeleted(discussionId))
            {
                throw new ResourceNotFoundException();
            }

            if (subArtifactId.HasValue && itemInfo.ArtifactId != artifactId)
            {
                throw new BadRequestException("Please provide a proper subartifact Id");
            }

            if (isDeleted)
            {
                revisionId = ((DeletedItemInfo)itemInfo).VersionId;
            }

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(new[] { artifactId }, Session.UserId, false, revisionId);
            var projectPermissions = await _artifactPermissionsRepository.GetProjectPermissions(itemInfo.ProjectId);

            RolePermissions permission = RolePermissions.None;
            if (!permissions.TryGetValue(artifactId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new AuthorizationException("You do not have permission to access the artifact");
            }
            var result = await _discussionsRepository.GetReplies(discussionId, itemInfo.ProjectId);
            foreach (var reply in result)
            {
                reply.CanDelete = !projectPermissions.HasFlag(ProjectPermissions.CommentsDeletionDisabled) && permissions.TryGetValue(artifactId, out permission) &&
                    (permission.HasFlag(RolePermissions.DeleteAnyComment) || (permission.HasFlag(RolePermissions.Comment) && reply.UserId == userId));
                reply.CanEdit = !projectPermissions.HasFlag(ProjectPermissions.CommentsModificationDisabled) &&
                    permissions.TryGetValue(artifactId, out permission) && (permission.HasFlag(RolePermissions.Comment) && reply.UserId == userId);
            }

            return result;
        }

        private void ValidateRequestParameters(int artifactId, int? subArtifactId)
        {
            if (artifactId < 1)
            {
                throw new BadRequestException(I18NHelper.FormatInvariant("Parameter: {0} is out of the range of valid values", nameof(artifactId)));
            }

            if (subArtifactId.HasValue && subArtifactId.Value < 1)
            {
                throw new BadRequestException(I18NHelper.FormatInvariant("Parameter: {0} is out of the range of valid values", nameof(subArtifactId)));
            }

            if (subArtifactId.HasValue && artifactId == subArtifactId.Value)
            {
                throw new BadRequestException("Please provide a proper subartifact Id");
            }
        }
    }
}
