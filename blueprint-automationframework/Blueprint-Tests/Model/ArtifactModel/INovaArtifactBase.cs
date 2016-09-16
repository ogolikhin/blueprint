using System;
using System.Collections.Generic;
using static Model.ArtifactModel.Impl.NovaArtifactDetails;

namespace Model.ArtifactModel
{
    public interface INovaArtifactBase
    {
        #region Serialized JSON Properties

        int Id { get; set; }
        int ItemTypeId { get; set; }
        string Name { get; set; }
        int ParentId { get; set; }
        int ProjectId { get; set; }
        int Version { get; set; }

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

        Identification CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string Description { get; set; }
        int ItemTypeVersionId { get; set; }
        int Permissions { get; set; }
        double OrderIndex { get; set; }
        Identification LastEditedBy { get; set; }
        DateTime? LastEditedOn { get; set; }
        Identification LockedByUser { get; set; } // this is an optional properties depending on state status of the target artifact
        DateTime? LockedDateTime { get; set; } // its existance depends on presense of LockedByUser property
        List<CustomProperty> CustomPropertyValues { get; }
        List<CustomProperty> SpecificPropertyValues { get; }

        #endregion Serialized JSON Properties
    }

    public interface INovaArtifactResponse : INovaArtifactBase
    {
        #region Serialized JSON Properties

        Identification CreatedBy { get; set; }
        DateTime? CreatedOn { get; set; }
        string Description { get; set; }
        Identification LastEditedBy { get; set; }
        DateTime? LastEditedOn { get; set; }
        int PredefinedType { get; set; }
        string Prefix { get; set; }

        #endregion Serialized JSON Properties
    }

}
