using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class BaseReviewArtifact
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public int ItemTypeId { get; set; }

        public int ItemTypePredefined { get; set; }

        public int? IconImageId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasComments { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasAttachments { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasRelationships { get; set; }
    }

    public class ReviewedArtifact : BaseReviewArtifact
    {
        public ViewStateType ViewState { get; set; }

        public string Approval { get; set; }

        public ApprovalType ApprovalFlag { get; set; }

        /// <summary>
        /// Viewed artifact version
        /// </summary>
        public int? ArtifactVersion { get; set; }

        /// <summary>
        /// e-signed by UserId on that UTC date time
        /// </summary>
        public DateTime? ESignedOn { get; set; }
    }

    public class ReviewedArtifacts
    {
        //public List<ReviewedArtifact> Items { get; set; }

        public int Total { get; set; }
    }
}