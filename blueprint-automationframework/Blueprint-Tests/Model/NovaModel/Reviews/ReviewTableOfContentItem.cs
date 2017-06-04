using Newtonsoft.Json;

namespace Model.NovaModel.Reviews
{
    // Taken from blueprint/svc/ArtifactStore/Models/Review/ReviewTableOfContent.cs
    public class ReviewTableOfContentItem : BaseReviewArtifact
    {
        public ApprovalType ApprovalStatus { get; set; }
        public int Level { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Included { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Viewed { get; set; }
    }
}
