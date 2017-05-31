using System;
using Newtonsoft.Json;

namespace Model.NovaModel.Reviews
{
    // see blueprint/svc/ArtifactStore/Models/Review/ReviewedArtifact.cs
    /// <summary>
    /// Artifact representation for Review Experience
    /// </summary>
    public class ReviewedArtifact : BaseReviewArtifact
    {
        public ViewStateType ViewState { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Approval { get; set; }

        public ApprovalType ApprovalFlag { get; set; }

        public int ArtifactVersion { get; set; }

        public DateTime PublishedOnTimestamp { get; set; }

        public string UserDisplayName { get; set; }
        /// <summary>
        /// Viewed artifact version
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public int? ViewedArtifactVersion { get; set; }

        /// <summary>
        /// e-signed by UserId on that UTC date time
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public DateTime? SignedOnTimestamp { get; set; }
    }
}
