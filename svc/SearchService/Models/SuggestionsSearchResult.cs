namespace SearchService.Models
{
    public class SuggestionsSearchResult: SearchResultSet<ArtifactSearchResult>
    {
        public int SourceId { get; set; }
    }
}