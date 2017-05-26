namespace ServiceLibrary.Models
{
    public class Pagination
    {
        public int? Offset { get; set; }

        public int? Limit { get; set; }

        public bool IsEmpty()
        {
            return Limit.HasValue && Limit.Value == 0;
        }
    }
}
