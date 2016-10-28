using System;
using System.Collections.Generic;

namespace ServiceLibrary.Models
{
    public interface IArtifact
    {
        int Id { get; set; }

        string Name { get; set; }

        int ProjectId { get; set; }

        int? ParentId { get; set; }

        int? ItemTypeId { get; set; }

        string Prefix { get; set; }

        ItemTypePredefined? PredefinedType { get; set; }

        int? Version { get; set; }

        double? OrderIndex { get; set; }

        bool? HasChildren { get; set; }

        RolePermissions? Permissions { get; set; }

        UserGroup LockedByUser { get; set; }

        DateTime? LockedDateTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For JSON serialization, the property sometimes needs to be null")]
        List<IArtifact> Children { get; set; }
    }
}
