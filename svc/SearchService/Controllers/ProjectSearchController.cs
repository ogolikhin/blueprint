using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace SearchService.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    public class ProjectSearchController : LoggableApiController
    {
        private readonly IProjectSearchRepository _projectSearchRepository;
        private const int MaxResultCount = 100;
        private const int DefaultResultCount = 20;
        private const string DefaultSeparator = "/";

        public override string LogSource => "SearchService.ProjectSearch";

        public ProjectSearchController() : this(new SqlProjectSearchRepository())
        {
        }
        public ProjectSearchController(IProjectSearchRepository projectSearchRepository)
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
        /// <param name="searchCriteria"></param>
        /// <param name="resultCount"></param>
        /// <param name="separatorChar"></param>
        /// <returns></returns>
        [HttpPost, NoCache]
        [Route("projectsearch"), SessionRequired]
        [ActionName("GetProjectsByName")]
        public async Task<IEnumerable<ProjectSearchResult>> GetProjectsByName(
            [FromBody] ProjectSearchCriteria searchCriteria, 
            int? resultCount = DefaultResultCount,
            string separatorChar = DefaultSeparator)
        {
            if (resultCount == null)
                resultCount = DefaultResultCount;

            if (string.IsNullOrEmpty(separatorChar))
                separatorChar = DefaultSeparator;

            if (resultCount > MaxResultCount)
                resultCount = MaxResultCount;

            if (string.IsNullOrEmpty(searchCriteria?.Query) || resultCount <= 0)
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

            return await _projectSearchRepository.GetProjectsByName(
                session.UserId, 
                searchCriteria.Query, 
                resultCount.Value,
                separatorChar);
        }
    }
}