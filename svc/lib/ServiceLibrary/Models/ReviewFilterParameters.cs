using System.Collections.Generic;
namespace ServiceLibrary.Models
{

    public class ReviewFilterParameters
    {
        public IEnumerable<int> ApprStsIds { get; set; }

        public bool? IsApprovalRequired { get; set; }

        public IEnumerable<string> ReviewStatuses { get; set; }

    }
}
