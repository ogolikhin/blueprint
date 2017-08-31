using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.LocalLog;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories.ConfigControl
{
    public class WorkflowServiceLogRepository : ServiceLogRepository
    {
        public WorkflowServiceLogRepository(IHttpClientProvider hcp, ILocalLog localLog, string configControlUri) :
            base(hcp, localLog, configControlUri)
        { }

        public override Task LogCLog(CLogModel logEntry)
        {
            try { return base.LogCLog(logEntry); }
            catch
            {
                return Task.Run(() => { });
            }
        }

        public override Task<HttpResponseMessage> LogClientMessage(ClientLogModel logEntry, string sessionId, string userName)
        {
            try { return base.LogClientMessage(logEntry, sessionId, userName); }
            catch
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }

        public override Task LogError(string source, Exception exception, string methodName = "", string filePath = "", int lineNumber = 0)
        {
            try { return base.LogError(source, exception, methodName, filePath, lineNumber); }
            catch
            {
                return Task.Run(() => { });
            }
        }

        public override Task LogError(string source, string message, string methodName = "", string filePath = "", int lineNumber = 0)
        {
            try { return base.LogError(source, message, methodName, filePath, lineNumber); }
            catch
            {
                return Task.Run(() => { });
            }
        }

        public override Task LogInformation(string source, string message, string methodName = "", string filePath = "", int lineNumber = 0)
        {
            try { return base.LogInformation(source, message, methodName, filePath, lineNumber); }
            catch
            {
                return Task.Run(() => { });
            }
        }

        public override Task LogPerformanceLog(PerformanceLogModel logEntry)
        {
            try { return base.LogPerformanceLog(logEntry); }
            catch
            {
                return Task.Run(() => { });
            }
        }

        public override Task LogSQLTraceLog(SQLTraceLogModel logEntry)
        {
            try { return base.LogSQLTraceLog(logEntry); }
            catch
            {
                return Task.Run(() => { });
            }
        }

        public override Task LogStandardLog(StandardLogModel logEntry)
        {
            try { return base.LogStandardLog(logEntry); }
            catch
            {
                return Task.Run(() => { });
            }
        }

        public override Task LogVerbose(string source, string message, string methodName = "", string filePath = "", int lineNumber = 0)
        {
            try { return base.LogVerbose(source, message, methodName, filePath, lineNumber); }
            catch
            {
                return Task.Run(() => { });
            }
        }

        public override Task LogWarning(string source, string message, string methodName = "", string filePath = "", int lineNumber = 0)
        {
            try
            {
                return base.LogWarning(source, message, methodName, filePath, lineNumber);
            }
            catch
            {
                return Task.Run(() => { });
            }
        }
    }
}
