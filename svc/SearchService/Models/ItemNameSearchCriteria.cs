using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SearchService.Models
{
    public class ItemNameSearchCriteria : SearchCriteria
    {
        [Required]
        public IEnumerable<int> ProjectIds { get; set; }

        public IEnumerable<int> PredefinedTypeIds { get; set; }
        
        public IEnumerable<int> ItemTypeIds { get; set; }
        public bool IncludeArtifactPath { get; set; }
    }
}
