using System.Collections.Generic;
namespace SearchService.Models
{
    public class SearchResultSet<T>
        where T: SearchResult
    {
        public IEnumerable<T> Items { get; set; } 
    }
}
