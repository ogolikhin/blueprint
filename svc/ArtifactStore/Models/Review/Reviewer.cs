using System;
using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class Reviewer
    {
        public int UserId { get; set; }

        public string DisplayName { get; set; }

        public ReviwerRole Role { get; set; }

        public ReviewStatus Status { get; set; }

        public DateTime? CompleteReviewDateTime { get; set; }

        public int Approved { get; set; }

        public int Disapproved { get; set; }

        public int Viewed { get; set; }
    }

    public class ReviewParticipantsContent
    {
        public IEnumerable<Reviewer> Items { get; set; }
        public int TotalArtifacts { get; set; }

        public int Total { get; set; }

        public int TotalArtifactsRequestedApproval { get; set; }
    }
}