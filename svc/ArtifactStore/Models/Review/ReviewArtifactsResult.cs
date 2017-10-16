using System;
using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifact : BaseReviewArtifact
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Disapproved { get; set; }
        public int Viewed { get; set; }
        public int Unviewed { get; set; }
        public bool HasMovedProject { get; set; }
    }

    internal class ReviewArtifactStatus
    {
        public int ArtifactId { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Disapproved { get; set; }
        public int Viewed { get; set; }
        public int Unviewed { get; set; }
    }

    internal class ContentStatusDetails
    {
        internal IEnumerable<ReviewArtifactStatus> ItemStatuses { get; set; }
        internal int NumUsers { get; set; }
        internal int NumApprovers { get; set; }
    }

    internal class PropertyValueString
    {
        public bool IsDraftRevisionExists { get; set; }
        public string ArtifactXml { get; set; }
        public int RevewSubartifactId { get; set; }
        public int? ProjectId { get; set; }
        public bool IsReviewLocked { get; set; }
        public ReviewPackageStatus ReviewStatus { get; set; }
        public int? BaselineId { get; set; }
        public bool IsReviewDeleted { get; set; }
        public bool? IsUserDisabled { get; set; }
        public ReviewType ReviewType { get; set; }
        public bool IsReviewReadOnly { get; set; }
    }
}