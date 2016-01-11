/// *************************************************************************************
/// ***** Any changes to this file need to be replicated in the                     *****
/// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
/// *************************************************************************************
using System;

namespace ServiceLibrary.Models
{
    public class BPLogEntry : IBPLogEntry
    {
        public string Source { get; set; }
        public LogLevelEnum LogLevel { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
        public string SessionId { get; set; }
        public string UserName { get; set; }
        public string StackTrace { get; set; }
    }
}
