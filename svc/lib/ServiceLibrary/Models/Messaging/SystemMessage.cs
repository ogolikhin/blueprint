namespace ServiceLibrary.Models.Messaging
{
    public class SystemMessage
    {
        public int? TargetJobId { get; set; }

        public SystemJobCommand Command { get; set; }
    }
}
