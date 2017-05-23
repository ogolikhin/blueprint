using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.NovaModel.Reviews
{
    // see blueprint/svc/ArtifactStore/Models/Review/ReviewArtifactDetails.cs
    public class ReviewArtifactDetails
    {
        public int UserId { get; set; }
        
        public string DisplayName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Approval { get; set; }

        public bool Viewed { get; set; }

        public DateTime? eSignatureTimestamp { get; set; }
    }

    public class ArtifactReviewContent
    {
        public List<ReviewArtifactDetails> Items { get; } = new List<ReviewArtifactDetails>();
        public int Total { get; set; }
    }
}
