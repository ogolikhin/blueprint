namespace ArtifactStore.Models.Review
{
    public class ParticipantArtifactStats
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public string Name { get; set; }
        public int ItemTypeId { get; set; }
        public int ItemTypePredefined { get; set; }
        public int? IconImageId { get; set; }
        public bool ArtifactRequiresApproval { get; set; }
        public ViewStateType ViewState { get; set; }
        public string ApprovalStatus { get; set; }
        public bool HasAccess { get; set; }

        public static explicit operator ParticipantArtifactStats(ReviewedArtifact reviewedArtifact)
        {
            return new ParticipantArtifactStats()
            {
                Id = reviewedArtifact.Id,
                Prefix = reviewedArtifact.Prefix,
                Name = reviewedArtifact.Name,
                ItemTypeId = reviewedArtifact.ItemTypeId,
                ItemTypePredefined = reviewedArtifact.ItemTypePredefined,
                IconImageId = reviewedArtifact.IconImageId,
                ArtifactRequiresApproval = reviewedArtifact.IsApprovalRequired,
                ApprovalStatus = reviewedArtifact.Approval,
                ViewState = GetViewState(reviewedArtifact),
                HasAccess = reviewedArtifact.HasAccess
            };
        }

        private static ViewStateType GetViewState(ReviewedArtifact reviewedArtifact)
        {
            if (!reviewedArtifact.ViewedArtifactVersion.HasValue)
            {
                return ViewStateType.NotViewed;
            }

            return reviewedArtifact.ViewedArtifactVersion == reviewedArtifact.ArtifactVersion ? ViewStateType.Viewed : ViewStateType.Changed;
        }
    }
}
