using System.Collections.Generic;

namespace SearchService.Models
{
    public class FullTextSearchResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public IEnumerable<FullTextSearchItem> FullTextSearchItems { get; set; }
    }
}