using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifact : BaseReviewArtifact
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Disapproved { get; set; }
        public int Viewed { get; set; }
        public int Unviewed { get; set; }
    }

    internal class ReviewArtifactStatus
    {
        public int ArtifactId { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Disapproved { get; set; }
        public int Viewed { get; set; }
        public int Unviewed { get; set; }
    }

    internal class ContentStatusDetails
    {
        internal IEnumerable<ReviewArtifactStatus> ItemStatuses { get; set; }
        internal int NumUsers { get; set; }
    }
}