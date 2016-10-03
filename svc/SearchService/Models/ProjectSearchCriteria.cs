using System.ComponentModel.DataAnnotations;

namespace SearchService.Models
{
    public class ProjectSearchCriteria
    {
        [Required]
        public string Query { get; set; }
    }
}