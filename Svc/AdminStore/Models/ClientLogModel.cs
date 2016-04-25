namespace AdminStore.Models
{
    //should map to ServiceLibrary.Models.ServiceLogModel
    public class ClientLogModel
    {
        public string Source { get; set; }
        public int LogLevel { get; set; }
        public string Message { get; set; }
        //public System.DateTime OccurredAt { get; set; }
        public string SessionId { get; set; }
        public string UserName { get; set; }
        public string MethodName { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string StackTrace { get; set; }
    }
}