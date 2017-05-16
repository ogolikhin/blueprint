namespace AdminStore.Models
{
    public class TableSettings
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string Filter { get; set; }
        public string Sort { get; set; }
    }
}