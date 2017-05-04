using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ArtifactStore.Models.Review
{
    [DataContract]
    public class Reviewer
    {
        [DataMember(Name = "Id")]
        public int UserId { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public ReviwerRole Role { get; set; }

        [DataMember]
        public ReviewStatus Status { get; set; }

        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CompleteReviewDateTime { get; set; }

        [DataMember]
        public int Approved { get; set; }

        [DataMember]
        public int Disapproved { get; set; }

        [DataMember]
        public int Viewed { get; set; }
    }

    public class ReviewParticipantsContent
    {
        public IEnumerable<Reviewer> Items { get; set; }
        public int TotalArtifacts { get; set; }

        public int Total { get; set; }

        public int TotalArtifactsRequestedApproval { get; set; }
    }
}