using System.Collections.Generic;

namespace SearchService.Models
{
    public class FullTextSearchResult
    {
        public IEnumerable<FullTextSearchItem> FullTextSearchItems { get; set; }

        public IEnumerable<FullTextSearchTypeItem> FullTextSearchTypeItems { get; set; }

        public int TotalCount { get; set; }

    }
}