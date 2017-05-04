using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.NovaModel.Impl
{
    // see blueprint/svc/ArtifactStore/Models/Review/Reviewer.cs
    public class Reviewer
    {
        [JsonProperty("Id")]
        public int UserId { get; set; }

        public string DisplayName { get; set; }

        public ReviewerRole Role { get; set; }

        public ReviewStatus Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public DateTime? CompleteReviewDateTime { get; set; }

        public int Approved { get; set; }

        public int Disapproved { get; set; }

        public int Viewed { get; set; }
    }

    public enum ReviewerRole
    {
        Reviewer = 0,
        Approver = 1
    }

    public class ReviewParticipantsContent
    {
        public List<Reviewer> Items { get; set; }

        public int TotalArtifacts { get; set; }

        public int Total { get; set; }

        public int TotalArtifactsRequestedApproval { get; set; }
    }
}
