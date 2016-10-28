namespace SearchService.Models
{
    public class ItemNameSearchResultSet : SearchResultSet<ItemNameSearchResult>
    {
        /// <summary>
        /// Number of items contained in this Page
        /// </summary>
        public int PageItemCount { get; set; }
    }
}
