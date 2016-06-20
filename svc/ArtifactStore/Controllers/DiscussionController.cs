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

        public override string LogSource { get; } = "ArtifactStore.ItemDiscussions";

        public DiscussionController() : this(new SqlDiscussionsRepository(), new SqlArtifactPermissionsRepository())
        {
        }
        public DiscussionController(IDiscussionsRepository discussionsRepository, IArtifactPermissionsRepository artifactPermissionsRepository) : base()
        {
            _discussionsRepository = discussionsRepository;
            _artifactPermissionsRepository = artifactPermissionsRepository;
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
        [Route("artifacts/{itemId:int:min(1)}/discussions"), SessionRequired]
        [ActionName("GetDiscussions")]
        public async Task<DiscussionResultSet> GetDiscussions(int itemId)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(new[] { itemId }, session.UserId);

            RolePermissions permission = RolePermissions.None;
            if (!permissions.TryGetValue(itemId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            var discussions = await _discussionsRepository.GetDiscussions(itemId, session.UserId, true);
            var result = new DiscussionResultSet
            {
                CanDelete = permission.HasFlag(RolePermissions.DeleteAnyComment),
                CanCreate = permission.HasFlag(RolePermissions.Comment),
                Discussions = discussions
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
        [Route("artifacts/{itemId:int:min(1)}/discussions/{discussionId:int:min(1)}/replies"), SessionRequired]
        [ActionName("GetReplies")]
        public async Task<IEnumerable<Reply>> GetReplies(int itemId, int discussionId)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;

            var permissions = await _artifactPermissionsRepository.GetArtifactPermissions(new[] { itemId }, session.UserId);

            RolePermissions permission = RolePermissions.None;
            if (!permissions.TryGetValue(itemId, out permission) || !permission.HasFlag(RolePermissions.Read))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            var result = await _discussionsRepository.GetReplies(discussionId, session.UserId, true);
            return result;
        }
    }
}