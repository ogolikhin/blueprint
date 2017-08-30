using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Models.Reuse
{
    public class SqlModifiedItems
    {
        public ModificationType ModificationType { get; set; }

        public int ArtifactId { get; set; }

        public int ItemId { get; set; }

        public int ProjectId { get; set; }

        public int Type { get; set; }

        public int? TypeId { get; set; }

        public int? VersionId { get; set; }

        public int? ArtifactId2 { get; set; }

        public int? ItemId2 { get; set; }

        public int? ProjectId2 { get; set; }
    }
}