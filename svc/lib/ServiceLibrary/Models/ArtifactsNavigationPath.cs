namespace ServiceLibrary.Models
{
    public class ArtifactsNavigationPath
    {
        public int Level { get; set; }
        public int ArtifactId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public int? ItemTypeId { get; set; }
    }
}
