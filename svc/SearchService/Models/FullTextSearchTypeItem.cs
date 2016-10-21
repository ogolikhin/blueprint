namespace SearchService.Models
{
    public class FullTextSearchTypeItem
    {
        public int ItemTypeId { get; set; }

        public string TypeName { get; set; }

        public int Count { get; set; }
    }
}