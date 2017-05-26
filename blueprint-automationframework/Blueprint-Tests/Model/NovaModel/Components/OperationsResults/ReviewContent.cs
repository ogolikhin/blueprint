using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.ArtifactModel.Impl.OperationsResults
{
    // see blueprint/svc/ArtifactStore/Models/Review/ReviewContent.cs
    public class ReviewArtifact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Prefix { get; set; }
        public int ItemTypeId { get; set; }
        public int ItemTypePredefined { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public int? IconImageId { get; set; }
        public bool HasComments { get; set; }
        public bool IsApprovalRequired { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Disapproved { get; set; }
        public int Viewed { get; set; }
        public int Unviewed { get; set; }
        public bool HasAccess { get; set; }
    }

    public class ReviewContent
    {
        public List<ReviewArtifact> Items { get; set; }
        public int Total { get; set; }
    }
}
