using Newtonsoft.Json;
using System.Collections.Generic;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Models.Review
{
    public class ReviewTableOfContentItem : BaseReviewArtifact
    {
        public ApprovalType ApprovalStatus { get; set; }
        public int Level { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Included { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int ArtifactVersion { get; set; }

        /// <summary>
        /// Viewed artifact version
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ViewedArtifactVersion { get; set; }

    }
}