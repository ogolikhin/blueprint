namespace AdminStore.Models
{
    public class AssignResult
    {
        public static AssignResult Empty => new AssignResult { TotalAssigned = 0 };
        public int TotalAssigned { get; set; }
    }
}