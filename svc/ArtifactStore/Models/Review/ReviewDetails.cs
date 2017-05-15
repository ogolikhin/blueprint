namespace ArtifactStore.Models.Review
{
    public class ReviewDetails
    {
        public int? BaselineId { get; set; }

        public string Prefix { get; set; }

        public string ArtifactType { get; set; }

        public ReviewParticipantRole? ReviewParticipantRole { get; set; }

        public ReviewPackageStatus ReviewPackageStatus { get; set; }

        public ReviewStatus ReviewStatus { get; set; }

        public int TotalReviewers { get; set; }

        public int TotalArtifacts { get; set; }

        public int Approved { get; set; }

        public int Disapproved { get; set; }

        public int Viewed { get; set; }

    }
}