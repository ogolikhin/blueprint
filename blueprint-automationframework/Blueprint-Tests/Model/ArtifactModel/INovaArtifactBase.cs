using Model.ArtifactModel.Impl;
using System;
using System.Collections.Generic;

namespace Model.ArtifactModel
{
    public interface INovaArtifactBase
    {
        #region Serialized JSON Properties

        int Id { get; set; }
        int ItemTypeId { get; set; }
        IUser LockedByUser { get; set; } // this is an optional properties depending on state status of the target artifact
        DateTime? LockedDateTime { get; set; } // its existance depends on presense of LockedByUser property
        string Name { get; set; }
        double OrderIndex { get; set; }
        int ParentId { get; set; }
        int Permissions { get; set; }
        int ProjectId { get; set; }
        int Version { get; set; }

        #endregion Serialized JSON Properties

    }

    public interface INovaArtifact : INovaArtifactBase
    {
        #region Serialized JSON Properties

        bool HasChildren { get; set; }
        int PredefinedType { get; set; }
        string Prefix { get; set; }

        #endregion Serialized JSON Properties
    }

    public interface INovaArtifactDetails : INovaArtifactBase
    {
        #region Serialized JSON Properties

        IUser CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        string Description { get; set; }
        int ItemTypeVersionId { get; set; }
        IUser LastEditedBy { get; set; }
        DateTime LastEditedOn { get; set; }
        List<PropertyForUpdate> CustomPropertyValues { get; }
        List<PropertyForUpdate> SpecificPropertyValues { get; }

        #endregion Serialized JSON Properties

    }

}
