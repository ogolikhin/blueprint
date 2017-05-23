﻿using System.Threading.Tasks;
using System.Web.Http;
using ArtifactStore.Models.Review;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ReviewContainersController : LoggableApiController
    {
        public override string LogSource { get; } = "ArtifactStore.Reviews";

        private IReviewsRepository _sqlReviewsRepository;

        public ReviewContainersController(): this(new SqlReviewsRepository())
        {
        }

        public ReviewContainersController(IReviewsRepository sqlReviewsRepository)
        {
            _sqlReviewsRepository = sqlReviewsRepository;
        }

        /// <summary>
        /// Gets the information about review container
        /// </summary>
        /// <remarks>
        /// Returns child artifacts of the artifact with the specified project and artifact ids.
        /// </remarks>
        /// <param name="containerId">Id of the review container</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the artifact.</response>
        /// <response code="404">Not found. An artifact for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{containerId:int:min(1)}"), SessionRequired]
        public Task<ReviewSummary> GetReviewContainerAsync(int containerId)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return _sqlReviewsRepository.GetReviewContainerAsync(containerId, session.UserId);
        }

        /// <summary>
        /// Gets review artifacts for a review given offset and limit, also returns total count.
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the review.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{containerId:int:min(1)}/content"), SessionRequired]
        public Task<ReviewArtifactsContent> GetContentAsync(int containerId, int? offset = 0, int? limit = 50, int? versionId = null)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return _sqlReviewsRepository.GetContentAsync(containerId, session.UserId, offset, limit, versionId);
        }

        /// <summary>
        /// Adds an artifact(s) to the specified review. Locks review if it is necessary.
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the artifact or it is locked by another user.</response>
        /// <response code="404">Not found. An artifact for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPut]
        [Route("containers/{reviewId:int:min(1)}/content"), SessionRequired]
        public Task<AddArtifactsResult> AddArtifactsToReview(int reviewId, [FromBody] AddArtifactsParameter content)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return _sqlReviewsRepository.AddArtifactsToReview(reviewId, session.UserId, content);
        }

        /// <summary>
        /// Gets review artifacts in a hierachy list for given review, offset and limit, also returns total count.
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="revisionId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions new the review</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{containerId:int:min(1)}/toc"), SessionRequired]
        public Task<ReviewTableOfContent> GetTableOfContentAsync(int containerId, int? revisionId = null, int? offset = 0, int? limit = 50)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return _sqlReviewsRepository.GetReviewTableOfContent(containerId, revisionId, session.UserId, offset, limit);
        }

        /// <summary>
        /// Gets review participants for a review given offset and limit, also returns total count.
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions new the review</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{containerId:int:min(1)}/participants"), SessionRequired]
        public Task<ReviewParticipantsContent> GetParticipantsAsync(int containerId, int? offset = 0, int? limit = 50, int? versionId = null)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return _sqlReviewsRepository.GetReviewParticipantsAsync(containerId, offset, limit, session.UserId, versionId);
        }

        /// <summary>
        /// Gets review participants for a review given offset and limit, also returns total count.
        /// </summary>
        /// <param name="artifactId"></param>
        /// <param name="containerId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions new the review</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{containerId:int:min(1)}/artifactreviewers/{artifactId=artifactId}"), SessionRequired]
        public Task<ArtifactReviewContent> GetReviewArtifactStatusesByParticipantAsync(int artifactId, int containerId, int? offset = 0, int? limit = 50, int? versionId = null)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return _sqlReviewsRepository.GetReviewArtifactStatusesByParticipant(artifactId, containerId, offset, limit, session.UserId, versionId);
        }
    }
}
