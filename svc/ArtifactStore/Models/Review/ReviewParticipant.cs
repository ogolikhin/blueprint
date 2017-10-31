using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceLibrary.Models;

namespace ArtifactStore.Models.Review
{
    [DataContract]
    public class ReviewParticipant
    {
        [DataMember(Name = "Id")]
        public int UserId { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public ReviewParticipantRole Role { get; set; }

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

        [DataMember]
        public IEnumerable<int> MeaningOfSignatureIds { get; set; }

        [DataMember]
        public IEnumerable<DropdownItem> PossibleMeaningOfSignatures { get; set; }
    }

    public class ReviewParticipantsContent
    {
        public IEnumerable<ReviewParticipant> Items { get; set; }
        public int TotalArtifacts { get; set; }

        public int Total { get; set; }

        public int TotalArtifactsRequestedApproval { get; set; }
    }
}