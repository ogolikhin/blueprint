using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models
{
    public class ArtifactBasicDetails
    {
        public int ItemId { get; set; }

        public int ArtifactId { get; set; }

        public int PrimitiveItemTypePredefined { get; set; }

        public int ProjectId { get; set; }

        public int? ParentId { get; set; }

        public double? OrderIndex { get; set; }

        public string Name { get; set; }

        public int ItemTypeId { get; set; }

        public string Prefix { get; set; }

        public int VersionsCount { get; set; }

        public int VersionIndex { get; set; }

        public bool DraftDeleted { get; set; }

        public bool LatestDeleted { get; set; }

        public bool HasDraftRelationships { get; set; }

        public int? UserId { get; set; }

        public string UserName { get; set; }

        public DateTime? LastSaveTimestamp { get; set; }

        public int? LockedByUserId { get; set; }

        public DateTime? LockedByUserTime { get; set; }

        public string LockedByUserName { get; set; }

        public int? LatestDeletedByUserId { get; set; }

        public DateTime? LatestDeletedByUserTime { get; set; }

        public string LatestDeletedByUserName { get; set; }
    }
}
