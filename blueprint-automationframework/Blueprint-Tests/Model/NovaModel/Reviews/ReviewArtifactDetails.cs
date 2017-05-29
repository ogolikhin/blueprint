using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Model.NovaModel.Reviews.Enums;

namespace Model.NovaModel.Reviews
{
    // see blueprint/svc/ArtifactStore/Models/Review/ReviewArtifactDetails.cs
    public class ReviewArtifactDetails
    {
        public int UserId { get; set; }

        public ReviewParticipantRole Role { get; set; }

        public string DisplayName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Approval { get; set; }

        public bool Viewed { get; set; }

        public DateTime? eSignatureTimestamp { get; set; }
    }
}
