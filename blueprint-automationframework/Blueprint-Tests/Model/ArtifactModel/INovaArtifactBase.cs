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
        IUser LockedByUser { get; set; }
        DateTime LockedDateTime { get; set; }
        string Name { get; set; }
        int OrderIndex { get; set; }
        int ParentId { get; set; }
        int Permissions { get; set; }
        int ProjectId { get; set; }
        int Version { get; set; }

        #endregion Serialized JSON Properties

    }

    public interface INovaArtifact
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
