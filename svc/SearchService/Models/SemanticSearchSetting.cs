namespace SearchService.Models
{
    public class SemanticSearchSetting
    {
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public string ConnectionString { get; set; }
        public SemanticSearchEngine SemanticSearchEngineType { get; set; }
    }
}