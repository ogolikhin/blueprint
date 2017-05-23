using System;
using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
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

    public class ReviewArtifactsDataSet
    {
        public IEnumerable<ReviewedArtifact> Items { get; set; }

        public int Total { get; set; }
    }
}