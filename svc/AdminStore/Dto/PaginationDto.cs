namespace AdminStore.Dto
{
    public class PaginationDto
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Count { get; set; }
    }
}