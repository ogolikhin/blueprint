using System.Collections.Generic;

namespace SearchService.Models
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
        /// Max number of items that can be contained within a page
        /// </summary>
        public int PageSize { get; set; }

        public IEnumerable<FullTextSearchTypeItem> FullTextSearchTypeItems { get; set; }
    }
}