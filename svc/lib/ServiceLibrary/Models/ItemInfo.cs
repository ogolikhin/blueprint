using System;

namespace ServiceLibrary.Models
{
    public class ItemInfo
    {
        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        public int ItemId { get; set; }
    }

    public class DeletedItemInfo : ItemInfo
    {
        public int VersionId { get; set; }

        public DateTime DeletedOn { get; set; }

        public int UserId { get; set; }
    }
}