using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ArtifactStore.Models;

namespace SearchService.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ProjectSearchController : LoggableApiController
    {
        private readonly IProjectSearchRepository _projectSearchRepository;
        private const int MaxResultCount = 100;
        private const int DefaultResultCount = 20;

        public override string LogSource => "SearchService.ProjectSearch";

        public ProjectSearchController() : this(new SqlProjectSearchRepository())
        {
        }
        public ProjectSearchController(IProjectSearchRepository projectSearchRepository) : base()
        {
            _projectSearchRepository = projectSearchRepository;
        }

        /// <summary>
        /// Get projects by name
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token or parameters are missing or malformed</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <param name="searchText"></param>
        /// <param name="resultCount"></param>
        /// <returns></returns>
        [HttpGet, NoCache]
        [Route("projectsearch"), SessionRequired]
        [ActionName("GetProjectsByName")]
        public async Task<IEnumerable<ProjectSearchResult>> GetProjectsByName(string searchText, int? resultCount = DefaultResultCount)
        {
            if (resultCount == null)
                resultCount = DefaultResultCount;

            if (resultCount > MaxResultCount)
                resultCount = MaxResultCount;

            if (searchText.Trim().Length <= 0)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            Session session = null;
            if (Request.Properties.Keys.Contains(ServiceConstants.SessionProperty))
                session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            if (session == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            return await _projectSearchRepository.GetProjectsByName(session.UserId, searchText, resultCount.Value);
        }


        /// <summary>
        /// Search artifacts by name
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token or parameters are missing or malformed</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <param name="searchText">Searching string</param>
        /// <param name="itemTypes">Artifact type filter parameter. Null if do not need search filter by artifact types.</param>
        /// <param name="resultCount">Number of results</param>
        /// <returns></returns>
        [HttpGet, NoCache]
        [Route("itemsearch"), SessionRequired]
        [ActionName("FindItemByName")]
        public async Task<IEnumerable<ItemSearchResult>> FindItemByName(string searchText, int[] projectIds,
            int[] itemTypes = null, int? resultCount = DefaultResultCount)
        {
            if (resultCount == null)
                resultCount = DefaultResultCount;

            if (resultCount > MaxResultCount)
                resultCount = MaxResultCount;

            if (searchText.Trim().Length <= 0)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            Session session = null;
            if (Request.Properties.Keys.Contains(ServiceConstants.SessionProperty))
                session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            if (session == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            return await _projectSearchRepository.FindItemByName(session.UserId, searchText, projectIds, itemTypes, resultCount.Value);
        }
    }
}