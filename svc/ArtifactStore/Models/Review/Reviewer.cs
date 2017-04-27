using System;

namespace ArtifactStore.Models.Review
{
    public class Reviewer
    {
        public int UserId { get; set; }

        public string DisplayName { get; set; }

        public ReviwerRole Role { get; set; }

        public ReviewStatus Status { get; set; }

        public DateTime? CompleteReviewDateTime { get; set; }
    }
}