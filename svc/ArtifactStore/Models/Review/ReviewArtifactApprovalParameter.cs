using ServiceLibrary.Models.ProjectMeta;
using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactApprovalParameter
    {
        public int ArtifactId { get; set; }

        public string Approval { get; set; }

        public ApprovalType ApprovalFlag { get; set; }
    }

   public class ReviewArtifactApprovalBulkParameter
    {
        public IEnumerable<int> ArtifactIds { get; set; }

        public string Approval { get; set; }

        public ApprovalType ApprovalFlag { get; set; }
        public bool IsBulk { get; set; }
        public int? RevisionId { get; set; }
    }
}
