using System.Collections.Generic;
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
            if (artifactId < 1 || (subArtifactId.HasValue && subArtifactId.Value < 1))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;

            var itemId = subArtifactId.HasValue ? subArtifactId.Value : artifactId;
            var revisionId = int.MaxValue;
            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(itemId);
            var itemInfo = isDeleted ?
                await _artifactVersionsRepository.GetDeletedItemInfo(itemId) :
                await _artifactPermissionsRepository.GetItemInfo(itemId, session.UserId, false);
            if (isDeleted)
            {
                revisionId = ((DeletedItemInfo)itemInfo).VersionId;
            }
            if (itemInfo == null)
            {
                throw new ResourceNotFoundException("You have attempted to access an item that does not exist or you do not have permission to view.",
                    subArtifactId.HasValue ? ErrorCodes.SubartifactNotFound : ErrorCodes.ArtifactNotFound);
            }

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(new[] { artifactId }, session.UserId, false, revisionId);
            var projectPermissions = await _artifactPermissionsRepository.GetProjectPermissions(itemInfo.ProjectId);

            RolePermissions permission = RolePermissions.None;
            if (!permissions.TryGetValue(artifactId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            var discussions = await _discussionsRepository.GetDiscussions(itemId, itemInfo.ProjectId);

            foreach (var discussion in discussions)
            {
                discussion.CanDelete = !projectPermissions.HasFlag(ProjectPermissions.CommentsDeletionDisabled)
                          && permissions.TryGetValue(artifactId, out permission) &&
                    (permission.HasFlag(RolePermissions.DeleteAnyComment) || (permission.HasFlag(RolePermissions.Comment) && discussion.UserId == session.UserId));
                discussion.CanEdit = !projectPermissions.HasFlag(ProjectPermissions.CommentsModificationDisabled) 
                          && permissions.TryGetValue(artifactId, out permission) && (permission.HasFlag(RolePermissions.Comment) && discussion.UserId == session.UserId);
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
            if (artifactId < 1 || discussionId < 1 || (subArtifactId.HasValue && subArtifactId.Value < 1))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;

            var itemId = subArtifactId.HasValue ? subArtifactId.Value : artifactId;
            var revisionId = int.MaxValue;
            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(itemId);
            var itemInfo = isDeleted ?
                await _artifactVersionsRepository.GetDeletedItemInfo(itemId):
                await _artifactPermissionsRepository.GetItemInfo(itemId, session.UserId);

            if (isDeleted)
            {
                revisionId = ((DeletedItemInfo)itemInfo).VersionId;
            }
            if (itemInfo == null || await _discussionsRepository.IsDiscussionDeleted(discussionId))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(new[] { artifactId }, session.UserId, false, revisionId);
            var projectPermissions = await _artifactPermissionsRepository.GetProjectPermissions(itemInfo.ProjectId);

            RolePermissions permission = RolePermissions.None;
            if (!permissions.TryGetValue(artifactId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            var result = await _discussionsRepository.GetReplies(discussionId, itemInfo.ProjectId);
            foreach (var reply in result)
            {
                reply.CanDelete = !projectPermissions.HasFlag(ProjectPermissions.CommentsDeletionDisabled) && permissions.TryGetValue(artifactId, out permission) &&
                    (permission.HasFlag(RolePermissions.DeleteAnyComment) || (permission.HasFlag(RolePermissions.Comment) && reply.UserId == session.UserId));
                reply.CanEdit = !projectPermissions.HasFlag(ProjectPermissions.CommentsModificationDisabled) &&
                    permissions.TryGetValue(artifactId, out permission) && (permission.HasFlag(RolePermissions.Comment) && reply.UserId == session.UserId);
            }

            return result;
        }
    }
}
