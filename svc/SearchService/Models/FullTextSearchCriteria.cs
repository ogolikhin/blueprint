using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SearchService.Models
{
    public class FullTextSearchCriteria : SearchCriteria
    {
        [Required]
        public IEnumerable<int> ProjectIds { get; set; }

        public IEnumerable<int> ItemTypeIds { get; set; }
    }
}