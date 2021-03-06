﻿// *************************************************************************************
// ***** Any changes to this file need to be replicated in the                     *****
// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
// *************************************************************************************
using ServiceLibrary.Models;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ConfigControl
{
    public interface IServiceLogRepository
    {
        Task LogError(string source, string message, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
        Task LogError(string source, Exception exception, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
        Task LogInformation(string source, string message, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
        Task LogVerbose(string source, string message, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
        Task LogWarning(string source, string message, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
        Task<HttpResponseMessage> LogClientMessage(ClientLogModel message, string sessionId, string userName);
        Task LogCLog(CLogModel logEntry);
        Task LogStandardLog(StandardLogModel logEntry);
        Task LogPerformanceLog(PerformanceLogModel logEntry);
        Task LogSQLTraceLog(SQLTraceLogModel logEntry);
    }
}