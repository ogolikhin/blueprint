using System;
using Newtonsoft.Json;

namespace ArtifactStore.Models.Review
{
    public class ReviewInfo
    {
        public int ItemId { get; set; }

        public ReviewPackageStatus ReviewStatus { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ExpiryTimestamp { get; set; }
    }
}