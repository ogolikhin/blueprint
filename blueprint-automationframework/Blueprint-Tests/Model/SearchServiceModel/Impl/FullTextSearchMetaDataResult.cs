using System.Collections.Generic;

namespace Model.SearchServiceModel.Impl
{
    public class FullTextSearchMetaDataResult
    {
        /// <summary>
        /// Total number of hits in the database
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of pages that will be returned
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Maximim number of items that can be contained within a page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The search artifact type items returned by the search
        /// </summary>
        public IEnumerable<FullTextSearchTypeItem> Items { get; set; }
    }
}