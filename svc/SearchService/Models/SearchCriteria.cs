using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SearchService.Models
{

    public class SearchCriteria
    {
        [Required]
        public int? UserId { get; set; }

        [Required]
        public string Query { get; set; }

        public IEnumerable<int> ProjectIds { get; set; }

        public IEnumerable<int> ItemTypeIds { get; set; }

    }
}