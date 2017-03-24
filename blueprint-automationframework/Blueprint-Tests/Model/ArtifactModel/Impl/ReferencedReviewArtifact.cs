using System;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    public class ReferencedReviewArtifact
    {
        public int ItemId { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ItemName { get; set; }
        public string ItemTypePrefix { get; set; }
    }

    public class ReviewRelationshipsResultSet
    {
        public List<ReferencedReviewArtifact> reviewArtifacts { get; } = new List<ReferencedReviewArtifact>();
    }
}
