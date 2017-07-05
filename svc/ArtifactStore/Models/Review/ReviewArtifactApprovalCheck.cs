using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactApprovalCheck
    {
        public bool ReviewExists { get; set; }
        public bool ReviewClosed { get; set; }
        public bool ReviewIsDraft { get; set; }
        public bool ReviewDeleted { get; set; }
        public bool AllArtifactsInReview { get; set; }
        public bool AllArtifactsRequireApproval { get; set; }
        public ReviewParticipantRole ReviewerRole { get; set; }
    }
}
