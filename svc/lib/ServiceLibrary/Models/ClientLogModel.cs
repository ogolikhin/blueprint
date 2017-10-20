namespace ServiceLibrary.Models
{
    // should map to ServiceLibrary.Models.ServiceLogModel
    public class ClientLogModel
    {
        public string Source { get; set; }
        public int LogLevel { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}