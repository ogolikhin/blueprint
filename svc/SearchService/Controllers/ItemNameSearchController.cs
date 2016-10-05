using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Helpers;
using ServiceLibrary.Attributes;
using ServiceLibrary.Models;
using SearchService.Helpers;

namespace SearchService.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("ItemNameSearch")]
    public class ItemNameSearchController : LoggableApiController
    {
        public override string LogSource => "SearchService.ItemNameSearch";
        public const int MaxResultCount = 100;

        private ISearchConfigurationProvider _searchConfigurationProvider;

        public ItemNameSearchController() : this(new SqlItemSearchRepository(), new SearchConfiguration())
        {
        }

        private readonly IItemSearchRepository _itemSearchRepository;
        public ItemNameSearchController(IItemSearchRepository itemSearchRepository, ISearchConfiguration configuration)
        {
            _itemSearchRepository = itemSearchRepository;

            _searchConfigurationProvider = new SearchConfigurationProvider(configuration);
        }

        #region Search

        /// <summary>
        /// Perform an Item search by Name
        /// </summary>
        /// <param name="searchCriteria">SearchCriteria object</param>
        /// <param name="startOffset">Search start offset</param>
        /// <param name="pageSize">Page Size</param>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request.</response>
        /// <response code="404">Not Found.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost, NoCache, SessionRequired]
        [Route("")]
        [ResponseType(typeof(ItemSearchResult))]
        public async Task<IHttpActionResult> Post([FromBody] ItemSearchCriteria searchCriteria, int? startOffset = null, int? pageSize = null)
        {
            // get the UserId from the session
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid || !ValidateSearchCriteria(searchCriteria))
            {
                return BadRequest();
            }

            int searchPageSize = pageSize.GetValueOrDefault(_searchConfigurationProvider.PageSize);
            if (searchPageSize <= 0)
            {
                searchPageSize = _searchConfigurationProvider.PageSize;
            }

            if (searchPageSize > MaxResultCount)
            {
                searchPageSize = MaxResultCount;
            }

            int searchStartOffset = startOffset.GetValueOrDefault(0);
            if (searchStartOffset < 0)
            {
                searchStartOffset = 0;
            }

            var results = await _itemSearchRepository.FindItemByName(userId.Value, searchCriteria, searchStartOffset, searchPageSize);
            
            results.PageItemCount = results.SearchItems.Count();

            return Ok(results);

        }

        #endregion        

        #region Private

        private int? GetUserId()
        {
            object sessionValue;
            if (!Request.Properties.TryGetValue(ServiceConstants.SessionProperty, out sessionValue))
            {
                return null;
            }
            var session = sessionValue as Session;
            return session?.UserId;
        }

        private bool ValidateSearchCriteria(ItemSearchCriteria searchCriteria)
        {
            if (string.IsNullOrWhiteSpace(searchCriteria?.Query) || 
                //searchCriteria.Query.Trim().Length < ServiceConstants.MinSearchQueryCharLimit || 
                !searchCriteria.ProjectIds.Any())
            {
                return false;
            }
            return true;
        }

        #endregion

    }
}
