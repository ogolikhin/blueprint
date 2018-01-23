using System.Collections.Generic;
namespace ArtifactStore.Models.Review
{

    public class ReviewFilterParameters
    {
        public IEnumerable<int> ApprStsIds { get; set; }

        public bool? IsApprovalRequired { get; set; }

        public IEnumerable<ReviewStatus> ReviewStatuses { get; set; }
    }
}
