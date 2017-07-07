namespace ServiceLibrary.Models.VersionControl
{
    public class SqlItemInfo
    {
        public int ItemId { get; set; }

        public int VersionProjectId { get; set; }

        public int? LockedByUserId { get; set; }

        public ItemTypePredefined PrimitiveItemTypePredefined { get; set; }

        public bool NotArtifact { get; set; }

        public bool NotFound { get; set; }

        public bool HasDraft { get; set; }

        public bool HasNeverBeenPublished { get; set; }

        public bool MovedAcrossProjects { get; set; }
    }
}
