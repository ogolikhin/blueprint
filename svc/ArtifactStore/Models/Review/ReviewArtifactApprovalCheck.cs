using System;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactApprovalCheck
    {
        public bool ReviewExists { get; set; }
        public ReviewPackageStatus ReviewStatus { get; set; }
        public bool ReviewDeleted { get; set; }
        public bool AllArtifactsInReview { get; set; }
        public bool AllArtifactsRequireApproval { get; set; }
        public bool UserInReview { get; set; }
        public ReviewParticipantRole ReviewerRole { get; set; }
        public ReviewType ReviewType { get; set; }
        public ReviewStatus ReviewerStatus { get; set; }

        private DateTime? _expirationDate;
        public DateTime? ExpirationDate
        {
            get
            {
                return _expirationDate;
            }
            set
            {
                if (value.HasValue)
                {
                    _expirationDate = value.Value.Kind != DateTimeKind.Utc ? value.Value.ToUniversalTime() : value;
                }
                else
                {
                    _expirationDate = value;
                }
            }
        }
    }
}
