using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ArtifactStore.Models.Review
{
    public class ArtifactReviewDetails
    {
        [DataMember(Name = "Id")]
        public int UserId { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Approval { get; set; }

        [DataMember]
        public bool Viewed { get; set; }

        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? eSignatureTimestamp { get; set; }
    }

    public class ArtifactReviewContent
    {
        public IEnumerable<ArtifactReviewDetails> Items { get; set; }
        public int Total { get; set; }

    }
}