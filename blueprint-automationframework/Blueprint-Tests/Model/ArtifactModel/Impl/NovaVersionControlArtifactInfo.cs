using System;

namespace Model.ArtifactModel.Impl
{
    public class NovaVersionControlArtifactInfo : INovaVersionControlArtifactInfo
    {
        #region Serialized JSON Properties

        public int Id { get; set; }
        public int? SubArtifactId { get; set; }
        public string Name { get; set; }
        public int? ProjectId { get; set; }
        public int? ParentId { get; set; }
        public int? ItemTypeId { get; set; }
        public string Prefix { get; set; }
        public int? PredefinedType { get; set; }
        public int? Version { get; set; }
        public int? VersionCount { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? HasChanges { get; set; }
        public double? OrderIndex { get; set; }
        public int? Permissions { get; set; }
        public NovaArtifactDetails.Identification LockedByUser { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public NovaArtifactDetails.Identification DeletedByUser { get; set; }
        public DateTime? DeletedDateTime { get; set; }

        #endregion Serialized JSON Properties
    }
}
