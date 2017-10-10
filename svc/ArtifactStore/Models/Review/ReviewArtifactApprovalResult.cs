using ServiceLibrary.Models.ProjectMeta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactApprovalResult
    {
        public IEnumerable<ArtifactApprovalResult> ApprovedArtifacts { get; set; }

        public bool? IsAllArtifactsProcessed { get; set; }
    }


    public class ArtifactApprovalResult
    {
        public int ArtifactId { get; set; }
        public DateTime? Timestamp { get; set; }
        public ApprovalType? PreviousApprovalFlag { get; set; }
    }
}
