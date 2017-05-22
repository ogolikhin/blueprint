using System;
using System.Collections.Generic;
using Model.NovaModel.Reviews.Enums;
using Newtonsoft.Json;

namespace Model.NovaModel.Reviews
{
    // see blueprint/svc/ArtifactStore/Models/Review/ReviewParticipant.cs
    public class ReviewParticipant
    {
        [JsonProperty("Id")]
        public int UserId { get; set; }

        public string DisplayName { get; set; }

        public ReviewerRole Role { get; set; }

        public ReviewStatus Status { get; set; }

        public DateTime? CompleteReviewDateTime { get; set; }

        public int Approved { get; set; }

        public int Disapproved { get; set; }

        public int Viewed { get; set; }
    }

    public class ReviewParticipantsContent
    {
        public List<ReviewParticipant> Items { get; set; }

        public int TotalArtifacts { get; set; }

        public int Total { get; set; }

        public int TotalArtifactsRequestedApproval { get; set; }
    }
}
