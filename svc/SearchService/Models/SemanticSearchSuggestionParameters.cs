namespace SearchService.Models
{
    public class SemanticSearchSuggestionParameters
    {
        public int ArtifactId { get; set; }
        public int UserId { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }

        public SemanticSearchSuggestionParameters(int artifactId, int userId, int pageSize = 10, int page = 0)
        {
            ArtifactId = artifactId;
            UserId = userId;
            PageSize = pageSize;
            Page = page;
        }
    }
}