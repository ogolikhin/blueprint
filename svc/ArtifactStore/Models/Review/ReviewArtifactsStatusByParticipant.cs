namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactsStatusByParticipant
    {
        public int Approved { get; set; }

        public int Disapproved { get; set; }

        public int Pending { get; set; }

        public int Viewed { get; set; }
    }
}