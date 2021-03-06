﻿namespace SearchService.Models
{
    public class FullTextSearchResultSet : SearchResultSet<FullTextSearchResult>
    {
        /// <summary>
        /// The page index
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Max number of items that can be returned for this page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Number of items contained in this Page
        /// </summary>
        public int PageItemCount { get; set; }
    }
}
