using ServiceLibrary.Models;

namespace SearchService.Models
{
    public class ArtifactSearchResult : SearchResult
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public int ProjectName { get; set; }

        public int ItemTypeId { get; set; }

        public string Prefix { get; set; }

        public int ItemTypeIconId { get; set; }

        public ItemTypePredefined PredefinedType { get; set; }
    }
}