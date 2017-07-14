using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace SearchService.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("projectsearch")]
    public class ProjectSearchController : LoggableApiController
    {
        private const int MaxResultCount = 101;
        private const int DefaultResultCount = 20;
        private const string DefaultSeparator = "/";

        private readonly IProjectSearchRepository _projectSearchRepository;

        public override string LogSource => "SearchService.ProjectSearch";

        public ProjectSearchController() : this(new SqlProjectSearchRepository(), new ServiceLogRepository())
        {
        }

        internal ProjectSearchController(IProjectSearchRepository projectSearchRepository, IServiceLogRepository serviceLogRepository) : base(serviceLogRepository)
        {
            _projectSearchRepository = projectSearchRepository;
        }

        /// <summary>
        /// Get projects by name
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="403">Forbidden. The user does not have permissions for the project.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <param name="searchCriteria"></param>
        /// <param name="resultCount"></param>
        /// <param name="separatorString"></param>
        /// <returns></returns>
        [HttpPost, NoCache, SessionRequired]
        [Route("name")]
        public async Task<ProjectSearchResultSet> SearchName(
            [FromBody] SearchCriteria searchCriteria, 
            int? resultCount = DefaultResultCount,
            string separatorString = DefaultSeparator)
        {
            if (resultCount == null)
                resultCount = DefaultResultCount;

            if (string.IsNullOrEmpty(separatorString))
                separatorString = DefaultSeparator;

            if (resultCount > MaxResultCount)
                resultCount = MaxResultCount;

            if (string.IsNullOrWhiteSpace(searchCriteria?.Query))
            {
                throw new BadRequestException("Please provide correct search criteria", ErrorCodes.IncorrectSearchCriteria);
            }

            if (resultCount <= 0)
            {
                throw new BadRequestException("Please provide correct result count", ErrorCodes.OutOfRangeParameter);
            }

            Session session = null;
            if (Request.Properties.Keys.Contains(ServiceConstants.SessionProperty))
                session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            if (session == null)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }

            try
            {
                return await _projectSearchRepository.SearchName(
                    session.UserId,
                    searchCriteria,
                    resultCount.Value,
                    separatorString);
            }
            catch (Exception ex)
            {
                await Log.LogError(LogSource, ex);
                throw;
            }
        }
    }
}
