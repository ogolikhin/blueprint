/// *************************************************************************************
/// ***** Any changes to this file need to be replicated in the                     *****
/// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
/// *************************************************************************************

namespace ServiceLibrary.Models
{
    public class ServiceLogEntry : IServiceLogEntry
    {
        public string Source { get; set; }
        public LogLevelEnum LogLevel { get; set; }
        public string Message { get; set; }
        public System.DateTime DateTime { get; set; }
        public string MethodName { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string StackTrace { get; set; }
    }
}
