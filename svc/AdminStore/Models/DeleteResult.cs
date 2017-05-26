namespace AdminStore.Models
{
    public class DeleteResult
    {
        public static DeleteResult Empty => new DeleteResult { TotalDeleted = 0 };

        public int TotalDeleted { get; set; }
    }
}