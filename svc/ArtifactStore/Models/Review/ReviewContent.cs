using System.Collections.Generic;
namespace ArtifactStore.Models.Review
{
    public class ReviewArtifact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public int ItemTypeId { get; set; }
        public int ItemTypePredefined { get; set; }
        public int? IconImageId { get; set; }
        public bool HasComments { get; set; }
        public bool IsApprovalRequired { get; set; }
        public int Pending { get; set; }
        public int Approved {get; set; }
        public int Disapproved { get; set; }
        public int Viewed { get; set; }
        public int Unviewed { get; set; }
        public bool HasAccess { get; set; }
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

    public class ReviewContent
    {
        public IEnumerable<ReviewArtifact> Items { get; set; }
        public int Total { get; set; }
    }
}