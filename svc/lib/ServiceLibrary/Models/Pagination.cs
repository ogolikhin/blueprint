namespace ServiceLibrary.Models
{
    public class Pagination
    {
        public int? Offset { get; set; }

        public int? Limit { get; set; }

        public void SetDefaultValues(int offset, int limit)
        {
            Offset = Offset ?? offset;
            Limit = Limit ?? limit;
        }
    }
}
