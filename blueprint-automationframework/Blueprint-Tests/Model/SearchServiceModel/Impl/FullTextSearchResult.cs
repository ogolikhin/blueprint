using System.Collections.Generic;

namespace Model.SearchServiceModel.Impl
{
    public class FullTextSearchResult
    {
        /// <summary>
        /// The page index
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Maximum number of items that can be returned for this page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Number of items contained in this Page
        /// </summary>
        public int PageItemCount { get; set; }

        /// <summary>
        /// The search items returned by the search
        /// </summary>
        public IEnumerable<FullTextSearchItem> Items { get; set; }
    }
}