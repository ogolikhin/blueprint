using System;
using System.Threading.Tasks;
using System.Web.Http;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Helpers;
using ServiceLibrary.Attributes;
using ServiceLibrary.Models;

namespace SearchService.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("FullTextSearch")]
    public class FullTextSearchController : LoggableApiController
    {
        public override string LogSource => "SearchService.FullTextSearch";

        public FullTextSearchController() : this(new SqlFullTextSearchRepository())
        {
        }

        private readonly IFullTextSearchRepository _fullTextSearchRepository;
        public FullTextSearchController(IFullTextSearchRepository fullTextSearchRepository)
        {
            _fullTextSearchRepository = fullTextSearchRepository;
        }

        /// <summary>
        /// Perform a Full Text Search
        /// </summary>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="page">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Not Found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost, NoCache, SessionRequired]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] SearchCriteria searchCriteria, int page = 1, int pageSize = -1)
        {
            // get the UserId from the session
            var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            searchCriteria.UserId = session?.UserId;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (pageSize == -1) pageSize = WebApiConfig.PageSize;

            var results = await _fullTextSearchRepository.Search(searchCriteria, page, pageSize);

            var totalPages = (double)results.TotalCount >= 0 ? Math.Ceiling((double)results.TotalCount / pageSize) : -1;

            return Ok(new
            {
                Page = page,
                TotalCount = results.TotalCount,
                TotalPages = totalPages,
                PageSize = pageSize,
                SearchResults = results.FullTextSearchItems,
                TypeResults = results.FullTextSearchTypeItems
            });
        }

    }
}
