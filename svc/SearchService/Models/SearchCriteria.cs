using System.ComponentModel.DataAnnotations;

namespace SearchService.Models
{
    public class SearchCriteria
    {
        [Required]
        public string Query { get; set; }
    }
}
