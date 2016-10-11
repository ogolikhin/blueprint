using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Attributes;
using SearchService.Helpers;

namespace SearchService.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("ItemNameSearch")]
    public class ItemNameSearchController : BaseSearchController
    {
        public override string LogSource => "SearchService.ItemNameSearch";
        public const int MaxResultCount = 100;
        private const string ArtifactPathStub = "Selected Project > Selected Folder > Selected Artifact";

        private ISearchConfigurationProvider _searchConfigurationProvider;

        public ItemNameSearchController() : this(new SqlItemSearchRepository(), new SearchConfiguration())
        {
        }

        private readonly IItemSearchRepository _itemSearchRepository;
        internal ItemNameSearchController(IItemSearchRepository itemSearchRepository, ISearchConfiguration configuration)
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
            var userId = ValidateAndExtractUserId();

            ValidateCriteria(searchCriteria);

            int searchPageSize = GetPageSize(_searchConfigurationProvider, pageSize, MaxResultCount);

            int searchStartOffset = GetStartCounter(startOffset, 0, 0);

            var results = await _itemSearchRepository.FindItemByName(userId, searchCriteria, searchStartOffset, searchPageSize);

            results.PageItemCount = results.SearchItems.Count();

            if (searchCriteria.IncludeArtifactPath)
            {
                // TODO Get Search Artifact Path
                foreach (var searchItem in results.SearchItems)
                {
                    searchItem.ArtifactPath = ArtifactPathStub;
                }
            }

            return Ok(results);

        }

        #endregion



    }
}
