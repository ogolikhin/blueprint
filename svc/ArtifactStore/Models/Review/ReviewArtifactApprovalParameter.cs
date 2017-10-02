using ServiceLibrary.Models.ProjectMeta;
using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{

   public class ReviewArtifactApprovalParameter
    {
        public IEnumerable<int> ArtifactIds { get; set; }

        public string Approval { get; set; }

        public ApprovalType ApprovalFlag { get; set; }
        public bool isExcludedArtifacts { get; set; }
        public int? RevisionId { get; set; }
    }
}
