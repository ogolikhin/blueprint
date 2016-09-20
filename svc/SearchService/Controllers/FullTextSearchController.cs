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
        [HttpPost, NoCache, NoSessionRequired]
        [Route("")]
        public async Task<IHttpActionResult> Post([FromBody] SearchCriteria searchCriteria, int page = 1, int pageSize = -1)
        {
            // get the UserId from the session
            //var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            //searchCriteria.UserId = session?.UserId;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (pageSize == -1) pageSize = WebApiConfig.PageSize;

            var results = await _fullTextSearchRepository.Search(searchCriteria, page, pageSize);

            results.Page = page;
            results.PageSize = pageSize;

            return Ok(results);
        }

        /// <summary>
        /// Return metadata for a Full Text Search
        /// </summary>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="pageSize">Page Size</param>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Not Found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost, NoCache, NoSessionRequired]
        [Route("metadata")]
        public async Task<IHttpActionResult> MetaData([FromBody] SearchCriteria searchCriteria, int pageSize = -1)
        {
            // get the UserId from the session
            //var session = Request.Properties[ServiceConstants.SessionProperty] as Session;
            //searchCriteria.UserId = session?.UserId;

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (pageSize == -1) pageSize = WebApiConfig.PageSize;

            var results = await _fullTextSearchRepository.SearchMetaData(searchCriteria);

            results.PageSize = pageSize;
            results.TotalPages = (int)results.TotalCount >= 0 ? (int)Math.Ceiling((double)results.TotalCount / pageSize) : -1;

            return Ok(results);
        }

    }
}
