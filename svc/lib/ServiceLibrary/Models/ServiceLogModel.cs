// *************************************************************************************
// ***** Any changes to this file need to be replicated in the                     *****
// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
// *************************************************************************************

namespace ServiceLibrary.Models
{
    public class ServiceLogModel
    {
        public string Source { get; set; }
        public LogLevelEnum LogLevel { get; set; }
        public string Message { get; set; }
        public System.DateTime OccuredAt { get; set; }
        public string SessionId { get; set; }
        public string UserName { get; set; }
        public string MethodName { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string StackTrace { get; set; }
    }
}
