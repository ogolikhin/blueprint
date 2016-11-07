using System;

namespace ServiceLibrary.Models
{
    public class ArtifactVersion
    {
        public int ItemId { get; set; }
        public int VersionProjectId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public double? OrderIndex { get; set; }
        public int StartRevision { get; set; }
        public int EndRevision { get; set; }
        public ItemTypePredefined? ItemTypePredefined { get; set; }
        public int? ItemTypeId { get; set; }
        public string Prefix { get; set; }
        public int? ItemTypeIconId { get; set; }
        public int? LockedByUserId { get; set; }
        public DateTime? LockedByUserTime { get; set; }
        public RolePermissions? DirectPermissions { get; set; }
        // Not returned in SQL server but calculated in the server application
        public RolePermissions? EffectivePermissions { get; set; }
        // Returned doubled if returned Head and Draft 
        public int? VersionsCount { get; set; }
        public bool HasDraft { get; set; }
        // Not returned in SQL server but calculated in the server application
        public bool? HasChildren { get; set; }
    }
}
