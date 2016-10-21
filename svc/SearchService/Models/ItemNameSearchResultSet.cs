using System.Collections.Generic;

namespace SearchService.Models
{
    public class ItemNameSearchResultSet : SearchResultSet<ItemSearchResult>
    {
        /// <summary>
        /// Number of items contained in this Page
        /// </summary>
        public int PageItemCount { get; set; }
    }
}
