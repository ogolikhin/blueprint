using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactApprovalParameter
    {
        public int ArtifactId { get; set; }

        public string Approval { get; set; }

        public ApprovalType ApprovalFlag { get; set; }
    }
}
