using Newtonsoft.Json;
using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class ReviewTableOfContentItem : BaseReviewArtifact
    {
        public ApprovalType ApprovalStatus { get; set; }
        public int Level { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Included { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool Viewed { get; set; }

    }

    public class ReviewTableOfContent
    {
        public IEnumerable<ReviewTableOfContentItem> Items { get; set; }
        public int Total { get; set; }
    }
}