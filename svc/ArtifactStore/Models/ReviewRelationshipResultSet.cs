using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models
{
    public class ReviewRelationshipsResultSet
    {
        public List<ReferencedReviewArtifact> reviewArtifacts { get; set; }
    }

    public class ReferencedReviewArtifact
    {
        public int itemId { get; set; }
        public string status { get; set; }
        public DateTime createdDate { get; set; }
    }
}