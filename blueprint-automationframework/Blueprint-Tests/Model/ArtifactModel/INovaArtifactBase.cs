using Model.ArtifactModel.Impl;
using System;
using System.Collections.Generic;
using Model.ModelHelpers;

namespace Model.ArtifactModel
{
    public interface INovaArtifactBase : INovaArtifactObservable, IHaveAnId
    {
        #region Serialized JSON Properties

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
        RolePermissions? Permissions { get; set; }
        int? PredefinedType { get; set; }
        double? OrderIndex { get; set; }
        string Prefix { get; set; }
        Identification LockedByUser { get; set; } // this is an optional properties depending on state status of the target artifact
        DateTime? LockedDateTime { get; set; } // its existance depends on presense of LockedByUser property

        /// <summary>
        /// A list of child artifacts.  This is optional and can be null depending on the REST call made.
        /// </summary>
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
        bool? LastSaveInvalid { get; set; }
        RolePermissions? Permissions { get; set; }
        double? OrderIndex { get; set; }
        Identification LastEditedBy { get; set; }
        DateTime? LastEditedOn { get; set; }
        Identification LockedByUser { get; set; } // this is an optional properties depending on state status of the target artifact
        DateTime? LockedDateTime { get; set; } // its existance depends on presense of LockedByUser property
        string Prefix { get; set; }
        List<CustomProperty> CustomPropertyValues { get; }
        List<CustomProperty> SpecificPropertyValues { get; }
        int? PredefinedType { get; set; }
        List<NovaTrace> Traces { get; set; }
        List<NovaSubArtifact> SubArtifacts { get; set; }

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
