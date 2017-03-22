using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models
{
    public class ReviewRelationshipsResultSet
    {
        public List<ReferencedReviewArtifact> ReviewArtifacts { get; internal set; }
    }

    public class ReferencedReviewArtifact
    {
        public int ItemId { get; set; }
        public int Status { get; set; }
        private DateTime _createdDate;
        public DateTime CreatedDate {
            get
            {
                return _createdDate;
            }
            set
            {
                _createdDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }
        public string itemName { get; set; }
        public string itemTypePrefix { get; set; }
    }
}