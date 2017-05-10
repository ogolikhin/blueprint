using System.Threading.Tasks;
using System.Web.Http;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ArtifactStore.Repositories;
using ServiceLibrary.Models;
using ArtifactStore.Models.Review;
using ServiceLibrary.Controllers;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ReviewContainersController : LoggableApiController
    {
        public override string LogSource { get; } = "ArtifactStore.Reviews";

        private SqlReviewsRepository _sqlReviewsRepository;

        public ReviewContainersController(): this(new SqlReviewsRepository())
        {
        }

        public ReviewContainersController(SqlReviewsRepository sqlReviewsRepository)
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
        public Task<ReviewContainer> GetReviewContainerAsync(int containerId)
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
        /// <response code="403">Forbidden. The user does not have permissions new the review</response>		
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("containers/{containerId:int:min(1)}/content"), SessionRequired]
        public Task<ReviewContent> GetContentAsync(int containerId, int? offset = 0, int? limit = 50, int? versionId = null)
        {
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            return _sqlReviewsRepository.GetContentAsync(containerId, session.UserId, offset, limit, versionId);
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
    }

    class ReviewArtifactStatus
    {
        public int ArtifactId { get; set; }

        public string Status { get; set; }
    }
}