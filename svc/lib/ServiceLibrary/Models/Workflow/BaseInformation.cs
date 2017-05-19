using System.ComponentModel.DataAnnotations;

namespace ServiceLibrary.Models.Workflow
{
    public class BaseInformation
    {
        public int Id { get; set; }

        [MinLength(1)]
        [MaxLength(128)]
        public string Name { get; set; }

        [MinLength(0)]
        [MaxLength(4000)]
        public string Description { get; set; }
    }
}