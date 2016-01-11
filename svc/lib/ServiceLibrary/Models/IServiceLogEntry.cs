/// *************************************************************************************
/// ***** Any changes to this file need to be replicated in the                     *****
/// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
/// *************************************************************************************
using System;

namespace ServiceLibrary.Models
{
    public interface IServiceLogEntry
    {
        string Source { get; set; }
        LogLevelEnum LogLevel { get; set; }
        string Message { get; set; }
        DateTime DateTime { get; set; }
        string MethodName { get; set; }
        string FilePath { get; set; }
        int LineNumber { get; set; }
        string StackTrace { get; set; }
    }
}