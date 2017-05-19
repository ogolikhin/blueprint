using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Model.NovaModel.Impl
{
    // see blueprint/svc/ArtifactStore/Models/Review/ArtifactReviewDetails.cs
    public class ArtifactReviewDetails
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
        public List<ArtifactReviewDetails> Items { get; } = new List<ArtifactReviewDetails>();
        public int Total { get; set; }
    }
}
