using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SearchService.Models
{

    public class ItemSearchCriteria
    {
        [Required]
        public string Query { get; set; }

        [Required]
        public IEnumerable<int> ProjectIds { get; set; }
       
        [Required]
        public int StartOffset { get; set; }

        /// <summary>
        /// Max number of items that can be returned for this page
        /// </summary>
        [Required]
        public int PageSize { get; set; }

        public IEnumerable<int> ItemTypeIds { get; set; }        
    }
}