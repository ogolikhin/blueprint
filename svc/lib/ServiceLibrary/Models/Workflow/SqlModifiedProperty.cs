namespace ServiceLibrary.Models.Workflow
{
    public class SqlModifiedProperty
    {
        public int ArtifactId { get; set; }
        public int ItemId { get; set; }
        public int ProjectId { get; set; }
        public int Type { get; set; }
        public int? TypeId { get; set; }
        public int VersionId { get; set; }
        public int StartRevision { get; set; }
        public int EndRevision { get; set; }
        public string PropertyName { get; set; }
        public string NewPropertyValue { get; set; }
    }
}
