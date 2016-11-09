using System;
using System.Collections.Generic;
using Model.ArtifactModel.Impl;
using System.Diagnostics.CodeAnalysis;

namespace Model.ArtifactModel
{
    public interface INovaArtifactBase : INovaArtifactObservable
    {
        #region Serialized JSON Properties

        int Id { get; set; }
        int? ItemTypeId { get; set; }
        string Name { get; set; }
        int? ParentId { get; set; }
        int? ProjectId { get; set; }
        int? Version { get; set; }

        #endregion Serialized JSON Properties
    }

    public interface INovaArtifact : INovaArtifactBase
    {
        #region Serialized JSON Properties

        bool HasChildren { get; set; }
        int Permissions { get; set; }
        int PredefinedType { get; set; }
        double OrderIndex { get; set; }
        string Prefix { get; set; }
        Identification LockedByUser { get; set; } // this is an optional properties depending on state status of the target artifact
        DateTime? LockedDateTime { get; set; } // its existance depends on presense of LockedByUser property

        /// <summary>
        /// A list of child artifacts.  This is optional and can be null depending on the REST call made.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] // This property can be null, so setter is needed.
        List<INovaArtifact> Children { get; set; }

        #endregion Serialized JSON Properties

        void AssertEquals(IArtifactBase artifact, bool shouldCompareVersions = true);
        void AssertEquals(INovaArtifactBase artifact);
    }

    public interface INovaArtifactDetails : INovaArtifactBase
    {
        #region Serialized JSON Properties

        List<AttachmentValue> AttachmentValues { get; }
        Identification CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string Description { get; set; }
        string ItemTypeName { get; set; }
        int? ItemTypeIconId { get; set; }
        int ItemTypeVersionId { get; set; }
        int Permissions { get; set; }
        double? OrderIndex { get; set; }
        Identification LastEditedBy { get; set; }
        DateTime? LastEditedOn { get; set; }
        Identification LockedByUser { get; set; } // this is an optional properties depending on state status of the target artifact
        DateTime? LockedDateTime { get; set; } // its existance depends on presense of LockedByUser property
        string Prefix { get; set; }
        List<CustomProperty> CustomPropertyValues { get; }
        List<CustomProperty> SpecificPropertyValues { get; }
        int? PredefinedType { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        List<NovaTrace> Traces { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        List<INovaSubArtifact> SubArtifacts { get; set; }

        #endregion Serialized JSON Properties
    }

    public interface INovaArtifactResponse : INovaArtifactBase
    {
        #region Serialized JSON Properties

        Identification CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string Description { get; set; }
        int? ItemTypeIconId { get; set; }
        Identification LastEditedBy { get; set; }
        DateTime? LastEditedOn { get; set; }
        double OrderIndex { get; set; }
        int PredefinedType { get; set; }
        string Prefix { get; set; }

        #endregion Serialized JSON Properties
    }

    public interface INovaProject
    {
        #region Serialized JSON Properties

        /// <summary>
        /// NOTE: Description should ALWAYS be returned as null because the Nova UI needs it to be null.
        /// </summary>
        string Description { get; set; }
        int Id { get; set; }
        string Name { get; set; }

        #endregion Serialized JSON Properties
    }

    public interface INovaArtifactsAndProjectsResponse
    {
        #region Serialized JSON Properties

        List<INovaArtifactResponse> Artifacts { get; }
        List<INovaProject> Projects { get; }

        #endregion Serialized JSON Properties
    }

    public interface INovaSubArtifact
    {
        #region Serialized JSON Properties
        int Id { get; set; }

        int ParentId { get; set; }

        int ItemTypeId { get; set; }

        string DisplayName { get; set; }

        int PredefinedType { get; set; }

        string Prefix { get; set; }

        bool HasChildren { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        List<INovaSubArtifact> Children { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        List<NovaTrace> Traces { get; set; }

        #endregion Serialized JSON Properties
    }

    public interface INovaVersionControlArtifactInfo : INovaArtifactBase
    {
        #region Serialized JSON Properties

        int? SubArtifactId { get; set; }
        string Prefix { get; set; }
        int? PredefinedType { get; set; }
        int? VersionCount { get; set; }
        bool? IsDeleted { get; set; }
        bool? HasChanges { get; set; }
        double? OrderIndex { get; set; }
        int? Permissions { get; set; }
        Identification LockedByUser { get; set; }
        DateTime? LockedDateTime { get; set; }
        Identification DeletedByUser { get; set; }
        DateTime? DeletedDateTime { get; set; }

        #endregion Serialized JSON Properties
    }
}
