// *************************************************************************************
// ***** Any changes to this file need to be replicated in the                     *****
// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
// *************************************************************************************
using Newtonsoft.Json;
using ServiceLibrary.Helpers;
using ServiceLibrary.LocalLog;
using ServiceLibrary.Models;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ConfigControl
{
    public class ServiceLogRepository : IServiceLogRepository
    {
        protected readonly IHttpClientProvider _httpClientProvider;
        protected readonly ILocalLog _localLog;
        protected readonly string _configControlUri;

        public ServiceLogRepository()
            : this(new HttpClientProvider(), new LocalFileLog(), ConfigurationManager.AppSettings["ConfigControl"])
        {
        }

        public ServiceLogRepository(IHttpClientProvider hcp, ILocalLog localLog, string configControlUri = null)
        {
            _httpClientProvider = hcp;
            _localLog = localLog;
            _configControlUri = configControlUri ?? ConfigurationManager.AppSettings["ConfigControl"];
        }

        /// <summary>
        /// LogInformation
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="message">message</param>
        /// <param name="methodName">Do not pass a value - compiler will fill this in</param>
        /// <param name="filePath">Do not pass a value - compiler will fill this in</param>
        /// <param name="lineNumber">Do not pass a value - compiler will fill this in</param>
        /// <example>
        /// var servicelog = new ServiceLogRepository();
        /// await servicelog.LogInformation("FileStore API", "Hello World");
        /// </example>
        public async Task LogInformation(
            string source,
            string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                var uri = _configControlUri;
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                var http = _httpClientProvider.Create(new Uri(uri));

                //create the log entry
                var logEntry = new ServiceLogModel
                {
                    LogLevel = LogLevelEnum.Informational,
                    Source = source,
                    Message = message,
                    OccurredAt = DateTime.Now,
                    MethodName = methodName,
                    FilePath = filePath,
                    LineNumber = lineNumber,
                    StackTrace = ""
                };

                // Convert Object to JSON
                var requestMessage = JsonConvert.SerializeObject(logEntry);
                var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await http.PostAsync("log", content);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _localLog.LogErrorFormat("Problem with ConfigControl Log service: {0}", ex.Message);
            }
        }

        /// <summary>
        /// LogVerbose
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="message">message</param>
        /// <param name="methodName">Do not pass a value - compiler will fill this in</param>
        /// <param name="filePath">Do not pass a value - compiler will fill this in</param>
        /// <param name="lineNumber">Do not pass a value - compiler will fill this in</param>
        /// <example>
        /// var servicelog = new ServiceLogRepository();
        /// await servicelog.LogVerbose("FileStore API", "Hello World");
        /// </example>
        public async Task LogVerbose(
            string source,
            string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                var uri = _configControlUri;
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                var http = _httpClientProvider.Create(new Uri(uri));

                //create the log entry
                var logEntry = new ServiceLogModel
                {
                    LogLevel = LogLevelEnum.Verbose,
                    Source = source,
                    Message = message,
                    OccurredAt = DateTime.Now,
                    MethodName = methodName,
                    FilePath = filePath,
                    LineNumber = lineNumber,
                    StackTrace = ""
                };

                // Convert Object to JSON
                var requestMessage = JsonConvert.SerializeObject(logEntry);
                var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await http.PostAsync("log", content);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _localLog.LogErrorFormat("Problem with ConfigControl Log service: {0}", ex.Message);
            }
        }

        /// <summary>
        /// LogWarning
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="message">message</param>
        /// <param name="methodName">Do not pass a value - compiler will fill this in</param>
        /// <param name="filePath">Do not pass a value - compiler will fill this in</param>
        /// <param name="lineNumber">Do not pass a value - compiler will fill this in</param>
        /// <example>
        /// var servicelog = new ServiceLogRepository();
        /// await servicelog.LogWarning("FileStore API", "Hello World");
        /// </example>
        public async Task LogWarning(
            string source,
            string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                var uri = _configControlUri;
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                var http = _httpClientProvider.Create(new Uri(uri));

                //create the log entry
                var logEntry = new ServiceLogModel
                {
                    LogLevel = LogLevelEnum.Warning,
                    Source = source,
                    Message = message,
                    OccurredAt = DateTime.Now,
                    MethodName = methodName,
                    FilePath = filePath,
                    LineNumber = lineNumber,
                    StackTrace = ""
                };

                // Convert Object to JSON
                var requestMessage = JsonConvert.SerializeObject(logEntry);
                var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await http.PostAsync("log", content);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _localLog.LogErrorFormat("Problem with ConfigControl Log service: {0}", ex.Message);
            }
        }

        /// <summary>
        /// LogClientMessage (should only be called from admin log controller)
        /// </summary>
        /// <param name="logEntry">message</param>
        /// <param name="sessionId">id of session (optional)</param>
        /// <param name="userName">user name (optional)</param>
        /// <example>
        /// var servicelog = new ServiceLogRepository();
        /// await servicelog.LogClientMessage("FileStore API", "Hello World");
        /// </example>
        public async Task<HttpResponseMessage> LogClientMessage(
            ClientLogModel logEntry,
            string sessionId,
            string userName)
        {
            //create the log entry
            StandardLogModel serviceLog = new StandardLogModel
            {
                Source = logEntry.Source,
                LogLevel = (LogLevelEnum)logEntry.LogLevel,
                Message = logEntry.Message,
                OccurredAt = DateTime.Now,
                SessionId = sessionId,
                UserName = userName,
                MethodName = "",
                FilePath = "",
                LineNumber = 0,
                StackTrace = logEntry.StackTrace
            };
            await LogStandardLog(serviceLog);
            //don't care about errors
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// LogError
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="message">message</param>
        /// <param name="methodName">Do not pass a value - compiler will fill this in</param>
        /// <param name="filePath">Do not pass a value - compiler will fill this in</param>
        /// <param name="lineNumber">Do not pass a value - compiler will fill this in</param>
        /// <example>
        /// var servicelog = new ServiceLogRepository();
        /// await servicelog.LogError("FileStore API", "Hello World");
        /// </example>
        public async Task LogError(
            string source,
            string message,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                var uri = _configControlUri;
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                var http = _httpClientProvider.Create(new Uri(uri));

                //create the log entry
                var logEntry = new ServiceLogModel
                {
                    LogLevel = LogLevelEnum.Error,
                    Source = source,
                    Message = message,
                    OccurredAt = DateTime.Now,
                    MethodName = methodName,
                    FilePath = filePath,
                    LineNumber = lineNumber,
                    StackTrace = ""
                };

                // Convert Object to JSON
                var requestMessage = JsonConvert.SerializeObject(logEntry);
                var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await http.PostAsync("log", content);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _localLog.LogErrorFormat("Problem with ConfigControl Log service: {0}", ex.Message);
            }
        }

        /// <summary>
        /// LogError
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="exception">Exception</param>
        /// <param name="methodName">Do not pass a value - compiler will fill this in</param>
        /// <param name="filePath">Do not pass a value - compiler will fill this in</param>
        /// <param name="lineNumber">Do not pass a value - compiler will fill this in</param>
        /// <example>
        /// var servicelog = new ServiceLogRepository();
        /// await servicelog.LogError("FileStore API", exception);
        /// </example>
        public async Task LogError(
            string source,
            Exception exception,
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                var uri = _configControlUri;
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                var http = _httpClientProvider.Create(new Uri(uri));

                //create the log entry
                var logEntry = new ServiceLogModel
                {
                    LogLevel = LogLevelEnum.Error,
                    Source = source,
                    Message = exception.Message,
                    OccurredAt = DateTime.Now,
                    MethodName = methodName,
                    FilePath = filePath,
                    LineNumber = lineNumber,
                    StackTrace = LogHelper.GetStackTrace(exception)
                };

                // Convert Object to JSON
                var requestMessage = JsonConvert.SerializeObject(logEntry);
                var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await http.PostAsync("log", content);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _localLog.LogErrorFormat("Problem with ConfigControl Log service: {0}", ex.Message);
            }
        }

        public async Task LogCLog(CLogModel logEntry)
        {
            try
            {
                var uri = _configControlUri;
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");

                var http = _httpClientProvider.Create(new Uri(uri));

                // Convert Object to JSON
                var requestMessage = JsonConvert.SerializeObject(logEntry);
                var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await http.PostAsync(@"log/clog", content);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _localLog.LogErrorFormat("Problem with ConfigControl Log service: {0}", ex.Message);
            }
        }

        public async Task LogStandardLog(StandardLogModel logEntry)
        {
            try
            {
                var uri = _configControlUri;
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                var http = _httpClientProvider.Create(new Uri(uri));

                // Convert Object to JSON
                var requestMessage = JsonConvert.SerializeObject(logEntry);
                var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await http.PostAsync(@"log/standardlog", content);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _localLog.LogErrorFormat("Problem with ConfigControl Log service: {0}", ex.Message);
            }
        }

        public async Task LogPerformanceLog(PerformanceLogModel logEntry)
        {
            try
            {
                var uri = _configControlUri;
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                var http = _httpClientProvider.Create(new Uri(uri));

                // Convert Object to JSON
                var requestMessage = JsonConvert.SerializeObject(logEntry);
                var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await http.PostAsync(@"log/performancelog", content);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _localLog.LogErrorFormat("Problem with ConfigControl Log service: {0}", ex.Message);
            }
        }

        public async Task LogSQLTraceLog(SQLTraceLogModel logEntry)
        {
            try
            {
                var uri = _configControlUri;
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                var http = _httpClientProvider.Create(new Uri(uri));

                // Convert Object to JSON
                var requestMessage = JsonConvert.SerializeObject(logEntry);
                var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await http.PostAsync(@"log/sqltracelog", content);

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _localLog.LogErrorFormat("Problem with ConfigControl Log service: {0}", ex.Message);
            }
        }
    }
}
