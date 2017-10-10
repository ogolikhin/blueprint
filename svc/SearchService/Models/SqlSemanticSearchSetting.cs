namespace SearchService.Models
{
    public class SqlSemanticSearchSetting
    {
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public string ElasticsearchConnectionString { get; set; }
        public SemanticSearchEngine SemanticSearchEngineType { get; set; }
    }
}