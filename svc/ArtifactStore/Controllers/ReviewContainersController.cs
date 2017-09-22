﻿using ArtifactStore.Models.Review;
using ArtifactStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using ServiceLibrary.Exceptions;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ReviewContainersController : LoggableApiController
    {
        private IReviewsRepository _sqlReviewsRepository;

        public override string LogSource { get; } = "ArtifactStore.Reviews";

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
        public Task<ReviewSummary> GetReviewSummary(int containerId)
        {
            return _sqlReviewsRepository.GetReviewSummary(containerId, Session.UserId);
        }

        /// <summary>
        /// Gets the artifacts for review experience
        /// </summary>
        /// <remarks>
        /// Returns child artifacts of the artifact with the specified project and artifact ids.
        /// </remarks>
        /// <param name="containerId">Id of the review container</param>
        /// <param name="pagination"></param>
        /// <param name="revisionId"></param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the artifact.</response>
        /// <response code="404">Not found. An artifact for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{containerId:int:min(1)}/artifacts"), SessionRequired]
        public Task<QueryResult<ReviewedArtifact>> GetReviewedArtifacts(int containerId, [FromUri] Pagination pagination, int? revisionId = int.MaxValue)
        {
            pagination.SetDefaultValues(0, 10);
            return _sqlReviewsRepository.GetReviewedArtifacts(containerId, Session.UserId, pagination, revisionId.Value);
        }

        /// <summary>
        /// Gets review artifacts for a review given offset and limit, also returns total count.
        /// </summary>
        /// <param name="containerId"></param>
        /// <param name="pagination"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the review.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{containerId:int:min(1)}/content"), SessionRequired]
        public Task<QueryResult<ReviewArtifact>> GetContentAsync(int containerId, [FromUri] Pagination pagination, int? versionId = null)
        {
            pagination.SetDefaultValues(0, 10);
            return _sqlReviewsRepository.GetReviewArtifactsContentAsync(containerId, Session.UserId, pagination, versionId);
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
            return _sqlReviewsRepository.AddArtifactsToReviewAsync(reviewId, Session.UserId, content);
        }

        /// <summary>
        /// Gets review artifacts in a hierachy list for given review, offset and limit, also returns total count.
        /// </summary>
        /// <param name="containerId">Review artifact Id</param>
        /// <param name="revisionId">Revision Id</param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions new the review</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{containerId:int:min(1)}/toc"), SessionRequired]
        public Task<QueryResult<ReviewTableOfContentItem>> GetTableOfContentAsync(int containerId, [FromUri] Pagination pagination, int? revisionId = int.MaxValue)
        {
            pagination.SetDefaultValues(0, 50);
            return _sqlReviewsRepository.GetReviewTableOfContent(containerId, revisionId.Value, Session.UserId, pagination);
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
            return _sqlReviewsRepository.GetReviewParticipantsAsync(containerId, offset, limit, Session.UserId, versionId);
        }

        /// <summary>
        /// Adds a participant(s) to the specified review.Locks review if it is necessary.
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
        [Route("containers/{reviewId:int:min(1)}/participants"), SessionRequired]
        public Task<AddParticipantsResult> AddParticipantsToReview(int reviewId, [FromBody] AddParticipantsParameter content)
        {
            return _sqlReviewsRepository.AddParticipantsToReviewAsync(reviewId, Session.UserId, content);
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
        public Task<QueryResult<ReviewArtifactDetails>> GetReviewArtifactStatusesByParticipantAsync(int artifactId, int containerId, int? offset = 0, int? limit = 50, int? versionId = null)
        {
            return _sqlReviewsRepository.GetReviewArtifactStatusesByParticipant(artifactId, containerId, offset, limit, Session.UserId, versionId);
        }

        /// <summary>
        /// Assigns ApprovalRequired flag for artifact in the review.
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the review or it is locked by another user.</response>
        /// <response code="404">Not found. An artifact for the specified id is not found, not a review, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPut]
        [Route("containers/{reviewId:int:min(1)}/artifacts/approval"), SessionRequired]
        public Task AssignApprovalRequiredToArtifacts(int reviewId, [FromBody] AssignArtifactsApprovalParameter content)
        {
            return _sqlReviewsRepository.AssignApprovalRequiredToArtifacts(reviewId, Session.UserId, content);
        }


        /// <summary>
        /// Assigns Roles to revievers in the review.
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the review or it is locked by another user.</response>
        /// <response code="404">Not found. An artifact for the specified id is not found, not a review, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPut]
        [Route("containers/{reviewId:int:min(1)}/reviewers/approval"), SessionRequired]
        public Task AssignRolesToReviewers(int reviewId, [FromBody] AssignReviewerRolesParameter content)
        {
            return _sqlReviewsRepository.AssignRolesToReviewers(reviewId, content, Session.UserId);
        }


        /// <summary>
        /// Returns an artifact index inside ra review content
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="artifactId"></param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the review or it is locked by another user.</response>
        /// <response code="404">Not found. An artifact for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{reviewId:int:min(1)}/index/{artifactId:int:min(1)}"), SessionRequired]
        public Task<ReviewArtifactIndex> GetReviewArtifactIndex(int reviewId, int artifactId, int? revisionId = int.MaxValue)
        {
            return _sqlReviewsRepository.GetReviewArtifactIndexAsync(reviewId, revisionId.Value, artifactId, Session.UserId);
        }

        /// <summary>
        /// Returns an artifact index inside of review's table of conent
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="artifactId"></param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the review or it is locked by another user.</response>
        /// <response code="404">Not found. An artifact for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{reviewId:int:min(1)}/toc/index/{artifactId:int:min(1)}"), SessionRequired]
        public Task<ReviewArtifactIndex> GetReviewTableOfContentArtifactIndex(int reviewId, int artifactId, int? revisionId = int.MaxValue)
        {
            return _sqlReviewsRepository.GetReviewTableOfContentArtifactIndexAsync(reviewId, revisionId.Value, artifactId, Session.UserId);
        }

        /// <summary>
        /// Sets the approval state of the given artifacts within a review for the session user.
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="reviewArtifactApprovalParameters"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the review or it is locked by another user.</response>
        /// <response code="404">Not found. An artifact for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPut, SessionRequired]
        [Route("containers/{reviewId:int:min(1)}/experience/approval")]
        public Task<IEnumerable<ReviewArtifactApprovalResult>> UpdateReviewArtifactApprovalAsync(int reviewId, [FromBody] IEnumerable<ReviewArtifactApprovalParameter> reviewArtifactApprovalParameters)
        {
            return _sqlReviewsRepository.UpdateReviewArtifactApprovalAsync(reviewId, reviewArtifactApprovalParameters, Session.UserId);
        }

        /// <summary>
        /// Sets the viewed state of the given artifact within a review for the session user.
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="artifactId"></param>
        /// <param name="viewedInput"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the review or it is locked by another user.</response>
        /// <response code="404">Not found. An artifact for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPut, SessionRequired]
        [Route("containers/{reviewId:int:min(1)}/artifacts/{artifactId:int:min(1)}/viewed")]
        public Task UpdateReviewArtifactViewedAsync(int reviewId, int artifactId, [FromBody] ReviewArtifactViewedInput viewedInput)
        {
            if (viewedInput == null || !viewedInput.Viewed.HasValue)
            {
                throw new BadRequestException("Viewed must be provided.");
            }

            return _sqlReviewsRepository.UpdateReviewArtifactViewedAsync(reviewId, artifactId, viewedInput.Viewed.Value, Session.UserId);
        }

        /// <summary>
        /// Get participant's review statistics for the given review and participant.
        /// </summary>
        /// <param name="reviewId">Review artifact Id</param>
        /// <param name="participantId">Participant Id</param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{reviewId:int:min(1)}/participants/{participantId:int:min(1)}/artifactstats"), SessionRequired]
        public Task<QueryResult<ParticipantArtifactStats>> GetReviewParticipantArtifactStatsAsync(int reviewId, int participantId, [FromUri] Pagination pagination)
        {
            return _sqlReviewsRepository.GetReviewParticipantArtifactStatsAsync(reviewId, participantId, Session.UserId, pagination);
        }

        /// <summary>
        /// Removes artifacts from a review for the session user.
        /// </summary>
        /// <param name="reviewId"></param>
        /// <param name="removeParams"></param>
        /// <returns></returns>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the review or it is locked by another user.</response>
        /// <response code="404">Not found. An artifact for the specified id is not found, does not exist or is deleted.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost, SessionRequired]
        [Route("containers/{reviewId:int:min(1)}/artifacts/remove")]
        public Task RemoveArtifactsFromReviewAsync(int reviewId, [FromBody] ReviewArtifactsRemovalParams removeParams)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return _sqlReviewsRepository.RemoveArtifactsFromReviewAsync(reviewId, removeParams, session.UserId);
        }

        [HttpPut, SessionRequired]
        [Route("containers/{reviewId:int:min(1)}/reviewer/status")]
        public Task UpdateReviewerStatusAsync(int reviewId, [FromBody] ReviewerStatusParameter reviewStatusParameter, int revisionId = int.MaxValue)
        {
            return _sqlReviewsRepository.UpdateReviewerStatusAsync(reviewId, revisionId, reviewStatusParameter.Status, Session.UserId);
        }
    }
}
