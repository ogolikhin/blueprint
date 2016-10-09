using System.Collections.Generic;

namespace SearchService.Models
{
    public interface ISearchCriteria
    {
        string Query { get; set; }

        IEnumerable<int> ProjectIds { get; set; }

        IEnumerable<int> ItemTypeIds { get; set; }
    }
}
