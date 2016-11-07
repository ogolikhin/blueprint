using System;

namespace Model.ArtifactModel.Impl
{
    public class NovaVersionControlArtifactInfo : NovaArtifactBase, INovaVersionControlArtifactInfo
    {
        #region Serialized JSON Properties

        public override int Id { get; set; }
        public int? SubArtifactId { get; set; }
        public override string Name { get; set; }
        public override int? ProjectId { get; set; }
        public override int? ParentId { get; set; }
        public override int? ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }
        public string Prefix { get; set; }
        public int? PredefinedType { get; set; }
        public override int? Version { get; set; }
        public int? VersionCount { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? HasChanges { get; set; }
        public double? OrderIndex { get; set; }
        public int? Permissions { get; set; }
        public Identification LockedByUser { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public Identification DeletedByUser { get; set; }
        public DateTime? DeletedDateTime { get; set; }

        #endregion Serialized JSON Properties
    }
}
