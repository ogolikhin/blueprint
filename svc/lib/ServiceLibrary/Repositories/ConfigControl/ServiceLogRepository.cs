﻿using Newtonsoft.Json;
using ServiceLibrary.Helpers;
using ServiceLibrary.LocalLog;
using ServiceLibrary.Models;
using System;
using System.Configuration;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ConfigControl
{
    public class ServiceLogRepository : IServiceLogRepository
    {
        internal readonly IHttpClientProvider _httpClientProvider;
        private readonly ILocalLog _localLog;

        public ServiceLogRepository()
            : this(new HttpClientProvider(), new LocalFileLog())
        {
        }

        public ServiceLogRepository(IHttpClientProvider hcp, ILocalLog localLog)
        {
            _httpClientProvider = hcp;
            _localLog = localLog;
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
                var uri = ConfigurationManager.AppSettings["ConfigControl"];
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                using (var http = _httpClientProvider.Create())
                {
                    http.BaseAddress = new Uri(uri);
                    http.DefaultRequestHeaders.Accept.Clear();

                    //create the log entry
                    LogEntry logEntry = new LogEntry(
                        LogLevelEnum.Informational,
                        source,
                        message,
                        methodName,
                        filePath,
                        lineNumber,
                        "");

                    // Convert Object to JSON
                    var requestMessage = JsonConvert.SerializeObject(logEntry);
                    var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await http.PostAsync("log", content);

                    response.EnsureSuccessStatusCode();
                }

            }
            catch (Exception ex)
            {
                _localLog.LogError(string.Format("Problem with ConfigControl Log service: {0}", ex.Message));
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
                var uri = ConfigurationManager.AppSettings["ConfigControl"];
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                using (var http = _httpClientProvider.Create())
                {
                    http.BaseAddress = new Uri(uri);
                    http.DefaultRequestHeaders.Accept.Clear();

                    //create the log entry
                    LogEntry logEntry = new LogEntry(
                        LogLevelEnum.Verbose,
                        source,
                        message,
                        methodName,
                        filePath,
                        lineNumber,
                        "");

                    // Convert Object to JSON
                    var requestMessage = JsonConvert.SerializeObject(logEntry);
                    var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await http.PostAsync("log", content);

                    response.EnsureSuccessStatusCode();
                }

            }
            catch (Exception ex)
            {
                _localLog.LogError(string.Format("Problem with ConfigControl Log service: {0}", ex.Message));
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
                var uri = ConfigurationManager.AppSettings["ConfigControl"];
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                using (var http = _httpClientProvider.Create())
                {
                    http.BaseAddress = new Uri(uri);
                    http.DefaultRequestHeaders.Accept.Clear();

                    //create the log entry
                    LogEntry logEntry = new LogEntry(
                        LogLevelEnum.Warning,
                        source,
                        message,
                        methodName,
                        filePath,
                        lineNumber,
                        "");

                    // Convert Object to JSON
                    var requestMessage = JsonConvert.SerializeObject(logEntry);
                    var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await http.PostAsync("log", content);

                    response.EnsureSuccessStatusCode();
                }

            }
            catch (Exception ex)
            {
                _localLog.LogError(string.Format("Problem with ConfigControl Log service: {0}", ex.Message));
            }
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
                var uri = ConfigurationManager.AppSettings["ConfigControl"];
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                using (var http = _httpClientProvider.Create())
                {
                    http.BaseAddress = new Uri(uri);
                    http.DefaultRequestHeaders.Accept.Clear();

                    //create the log entry
                    LogEntry logEntry = new LogEntry(
                        LogLevelEnum.Error,
                        source,
                        message,
                        methodName,
                        filePath,
                        lineNumber,
                        "");

                    // Convert Object to JSON
                    var requestMessage = JsonConvert.SerializeObject(logEntry);
                    var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await http.PostAsync("log", content);

                    response.EnsureSuccessStatusCode();
                }

            }
            catch (Exception ex)
            {
                _localLog.LogError(string.Format("Problem with ConfigControl Log service: {0}", ex.Message));
            }
        }

        /// <summary>
        /// LogError
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="ex">Exception</param>
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
                var uri = ConfigurationManager.AppSettings["ConfigControl"];
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
                using (var http = _httpClientProvider.Create())
                {
                    http.BaseAddress = new Uri(uri);
                    http.DefaultRequestHeaders.Accept.Clear();

                    //create the log entry
                    LogEntry logEntry = new LogEntry(
                        LogLevelEnum.Error,
                        source,
                        exception.Message,
                        methodName,
                        filePath,
                        lineNumber,
                        GetStackTrace(exception));

                    // Convert Object to JSON
                    var requestMessage = JsonConvert.SerializeObject(logEntry);
                    var content = new StringContent(requestMessage, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await http.PostAsync("log", content);

                    response.EnsureSuccessStatusCode();
                }

            }
            catch (Exception ex)
            {
                _localLog.LogError(string.Format("Problem with ConfigControl Log service: {0}", ex.Message));
            }
        }

        private string GetStackTrace(Exception ex)
        {
            var stringBuilder = new StringBuilder();

            while (ex != null)
            {
                stringBuilder.AppendLine(ex.Message);
                stringBuilder.AppendLine(ex.StackTrace);

                ex = ex.InnerException;
            }

            return stringBuilder.ToString();
        }
    }
}


