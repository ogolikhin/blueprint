using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceLibrary.Models;

namespace SearchService.Models
{
    public class ItemSearchResult : SearchResult, IArtifact
    {
        public int Id {
            get { return ItemId; }
            set { ItemId = value; }
        }

        public int ProjectId { get; set; }

        public int? ParentId { get; set; }

        public int? ItemTypeId { get; set; }

        public string Prefix { get; set; }

        public ItemTypePredefined? PredefinedType { get; set; }

        public int? Version { get; set; }

        public double? OrderIndex { get; set; }

        public bool? HasChildren { get; set; }

        public RolePermissions? Permissions { get; set; }

        public UserGroup LockedByUser { get; set; }

        [IgnoreDataMember]
        public int? LockedByUserId { get; set; }

        public DateTime? LockedDateTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For JSON serialization, the property sometimes needs to be null")]
        public List<IArtifact> Children { get; set; }
    }
}
