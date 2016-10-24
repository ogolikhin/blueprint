namespace SearchService.Models
{
    public class ItemSearchResult : SearchResult
    {
        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        public int ItemTypeId { get; set; }

        public string TypeName { get; set; }

        public string TypePrefix { get; set; }
    }
}
