using ArtifactStore.Helpers;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Filters;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Net;
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
        /// <response code="400">Bad Request. The session token or parameters are missing or malformed</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
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
            var itemInfo = await _artifactPermissionsRepository.GetItemInfo(itemId, session.UserId);
            var revisionId = int.MaxValue;
            if (itemInfo == null && await _artifactVersionsRepository.IsItemDeleted(itemId))
            {
                itemInfo = await _artifactVersionsRepository.GetDeletedItemInfo(itemId);
                revisionId = ((DeletedItemInfo)itemInfo).VersionId;
            }
            if (itemInfo == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(new[] { itemId }, session.UserId, false, revisionId);

            RolePermissions permission = RolePermissions.None;
            if (!permissions.TryGetValue(itemId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            var discussions = await _discussionsRepository.GetDiscussions(itemId, itemInfo.ProjectId);

            foreach (var discussion in discussions)
            {
                discussion.CanDelete = permissions.TryGetValue(discussion.ItemId, out permission) &&
                    (permission.HasFlag(RolePermissions.DeleteAnyComment) || (permission.HasFlag(RolePermissions.Comment) && discussion.UserId == session.UserId));
                discussion.CanEdit = permissions.TryGetValue(discussion.ItemId, out permission) && (permission.HasFlag(RolePermissions.Comment) && discussion.UserId == session.UserId);
            }
            
            var result = new DiscussionResultSet
            {
                CanDelete = permission.HasFlag(RolePermissions.DeleteAnyComment) && revisionId == int.MaxValue,
                CanCreate = permission.HasFlag(RolePermissions.Comment) && revisionId == int.MaxValue,
                Discussions = discussions,
                EmailDiscussionsEnabled = await _discussionsRepository.AreEmailDiscussionsEnabled(itemInfo.ProjectId)
            };
            return result;
        }

        /// <summary>
        /// Get item discussions
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token or parameters are missing or malformed</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
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
            var itemInfo = await _artifactPermissionsRepository.GetItemInfo(itemId, session.UserId);
            var revisionId = int.MaxValue;
            if (itemInfo == null && await _artifactVersionsRepository.IsItemDeleted(itemId))
            {
                itemInfo = await _artifactVersionsRepository.GetDeletedItemInfo(itemId);
                revisionId = ((DeletedItemInfo)itemInfo).VersionId;
            }
            if (itemInfo == null || await _discussionsRepository.IsDiscussionDeleted(discussionId))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(new[] { itemId }, session.UserId, false, revisionId);

            RolePermissions permission = RolePermissions.None;
            if (!permissions.TryGetValue(itemId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            var result = await _discussionsRepository.GetReplies(discussionId, itemInfo.ProjectId);
            foreach (var reply in result)
            {
                reply.CanDelete = permissions.TryGetValue(reply.ItemId, out permission) &&
                    (permission.HasFlag(RolePermissions.DeleteAnyComment) || (permission.HasFlag(RolePermissions.Comment) && reply.UserId == session.UserId));
                reply.CanEdit = permissions.TryGetValue(reply.ItemId, out permission) && (permission.HasFlag(RolePermissions.Comment) && reply.UserId == session.UserId);
            }

            return result;
        }
    }
}