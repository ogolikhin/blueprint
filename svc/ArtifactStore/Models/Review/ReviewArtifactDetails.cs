﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifactDetails
    {
        [DataMember(Name = "Id")]
        public int UserId { get; set; }

        [DataMember]
        public ReviewParticipantRole Role { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Approval { get; set; }

        [DataMember]
        public ViewStateType ViewState { get; set; }

        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? eSignatureTimestamp { get; set; }
    }

    public class ArtifactReviewContent
    {
        public IEnumerable<ReviewArtifactDetails> Items { get; set; }
        public int Total { get; set; }

    }
}