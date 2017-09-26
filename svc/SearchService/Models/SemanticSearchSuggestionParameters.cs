namespace SearchService.Models
{
    public class SemanticSearchSuggestionParameters
    {
        public int ArtifactId { get; }
        public int UserId { get; }
        public int PageSize { get; }
        public int Page { get; }

        public SemanticSearchSuggestionParameters(int artifactId, int userId, int pageSize = 10, int page = 0)
        {
            ArtifactId = artifactId;
            UserId = userId;
            PageSize = pageSize;
            Page = page;
        }
    }
}