using System;
using System.Collections.Generic;

namespace ArtifactStore.Models.Review
{
    public class ReviewArtifact : BaseReviewArtifact
    {
        public bool IsApprovalRequired { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Disapproved { get; set; }
        public int Viewed { get; set; }
        public int Unviewed { get; set; }
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
    }

    internal class PropertyValueVersions
    {
      public int  VersionId { get; set; }
        public int VersionUserId { get; set; }
        public int? VersionProjectId { get; set; }
        public int? VersionArtifactId { get; set; }
        public int? VersionItemId { get; set; }
        public int StartRevision { get; set; }
        public int EndRevision { get; set; }
        public int PropertyTypePredefined { get; set; }
        public int PrimitiveType { get; set; }
        public decimal? DecimalValue{ get; set; }
        public DateTime? DateValue { get; set; }
        public string UserValue { get; set; }
        public string UserLabel { get; set; }
        public string StringValue { get; set; }
        public string CustomPropertyChar { get; set; }
        public int ImageValue_ImageId { get; set; }
        public int PropertyType_PropertyTypeId { get; set; }
        public decimal? NumericValue { get; set; }
        public string CustomProperty { get; set; }
}
}