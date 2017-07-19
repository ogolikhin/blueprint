namespace ArtifactStore.Models.Review
{
    public class ParticipantArtifactStats
    {
        public string ArtifactId { get; set; }
        public string ArtifactName { get; set; }
        public bool ArtifactRequiresApproval { get; set; }
        public bool Viewed { get; set; }
        public string ApprovalStatus { get; set; }
        public bool HasAccess { get; set; }

        public static explicit operator ParticipantArtifactStats(ReviewedArtifact reviewedArtifact)
        {
            return new ParticipantArtifactStats()
            {
                ArtifactId = reviewedArtifact.Prefix + reviewedArtifact.Id,
                ArtifactName = reviewedArtifact.Name,
                ArtifactRequiresApproval = reviewedArtifact.IsApprovalRequired,
                ApprovalStatus = reviewedArtifact.Approval,
                Viewed = reviewedArtifact.ViewedArtifactVersion == reviewedArtifact.ArtifactVersion,
                HasAccess = reviewedArtifact.HasAccess
            };
        }
    }
}
