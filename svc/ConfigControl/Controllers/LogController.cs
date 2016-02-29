﻿using System.Configuration;
using System.Net;
using System.Net.Http;
using ServiceLibrary.EventSources;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using ConfigControl.Repositories;

namespace ConfigControl.Controllers
{
    [RoutePrefix("log")]
    public class LogController : ApiController
    {
        internal readonly IHttpClientProvider _httpClientProvider;

        public LogController() : this(new HttpClientProvider())
        {
        }

        internal LogController(IHttpClientProvider httpClientProvider)
        {
            _httpClientProvider = httpClientProvider;
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <remarks>
        /// Sends a <paramref name="logEntry" /> coming from Blueprint Services to the logging service
        /// </remarks>
        /// <param name="logEntry">Log entry</param>
        /// <response code="200">OK.</response>
        [HttpPost]
        [Route("")]
        public IHttpActionResult Log([FromBody]ServiceLogModel logEntry)
        {
            string ipAdress = GetIpAddress();

            switch (logEntry.LogLevel)
            {
                case LogLevelEnum.Error:
                    BlueprintEventSource.Log.Error(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.OccurredAt,
                        logEntry.MethodName,
                        logEntry.FilePath,
                        logEntry.LineNumber,
                        logEntry.StackTrace);
                    break;
                case LogLevelEnum.Warning:
                    BlueprintEventSource.Log.Warning(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.OccurredAt,
                        logEntry.MethodName,
                        logEntry.FilePath,
                        logEntry.LineNumber);
                    break;
                case LogLevelEnum.Informational:
                    BlueprintEventSource.Log.Informational(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.OccurredAt,
                        logEntry.MethodName,
                        logEntry.FilePath,
                        logEntry.LineNumber);
                    break;
                case LogLevelEnum.Verbose:
                    BlueprintEventSource.Log.Verbose(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.OccurredAt,
                        logEntry.MethodName,
                        logEntry.FilePath,
                        logEntry.LineNumber);
                    break;
                default:
                    break;
            }

            return Ok();
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <remarks>
        /// Sends a <paramref name="logEntry" /> coming from Silverlight client to the logging service
        /// </remarks>
        /// <param name="logEntry">Log entry</param>
        /// <response code="200">OK.</response>
        [HttpPost]
        [Route("CLog")]
        public IHttpActionResult Log([FromBody]CLogModel logEntry)
        {
            string ipAdress = GetIpAddress();

            if (string.IsNullOrEmpty(logEntry.ActionName))
            {
                switch (logEntry.LogLevel)
                {
                    case LogLevelEnum.Error:
                        CLogEventSource.Log.Error(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.StackTrace,
                            logEntry.OccurredAt,
                            logEntry.UserName);
                        break;
                    case LogLevelEnum.Warning:
                        CLogEventSource.Log.Warning(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccurredAt,
                            logEntry.UserName);
                        break;
                    case LogLevelEnum.Informational:
                        CLogEventSource.Log.Informational(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccurredAt,
                            logEntry.UserName);
                        break;
                    case LogLevelEnum.Verbose:
                        CLogEventSource.Log.Verbose(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccurredAt,
                            logEntry.UserName);
                        break;
                    case LogLevelEnum.Critical:
                        CLogEventSource.Log.Critical(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.StackTrace,
                            logEntry.OccurredAt,
                            logEntry.UserName);
                        break;
                    default:
                        break;
                }

            }
            else
            {
                switch (logEntry.LogLevel)
                {
                    case LogLevelEnum.Error:
                        CLogEventSource.Log.ErrorPerf(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.StackTrace,
                            logEntry.OccurredAt,
                            logEntry.UserName,
                            logEntry.ActionName,
                            logEntry.Duration);
                        break;
                    case LogLevelEnum.Warning:
                        CLogEventSource.Log.WarningPerf(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccurredAt,
                            logEntry.UserName,
                            logEntry.ActionName,
                            logEntry.Duration);
                        break;
                    case LogLevelEnum.Informational:
                        CLogEventSource.Log.InformationalPerf(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccurredAt,
                            logEntry.UserName,
                            logEntry.ActionName,
                            logEntry.Duration);
                        break;
                    case LogLevelEnum.Verbose:
                        CLogEventSource.Log.VerbosePerf(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccurredAt,
                            logEntry.UserName,
                            logEntry.ActionName,
                            logEntry.Duration);
                        break;
                    case LogLevelEnum.Critical:
                        CLogEventSource.Log.CriticalPerf(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.StackTrace,
                            logEntry.OccurredAt,
                            logEntry.UserName,
                            logEntry.ActionName,
                            logEntry.Duration);
                        break;
                    default:
                        break;
                }
            }

            return Ok();
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <remarks>
        /// Sends a <paramref name="logEntry" /> coming from Blueprint Server Logger to the logging service
        /// </remarks>
        /// <param name="logEntry">Log entry</param>
        /// <response code="200">OK.</response>
        [HttpPost]
        [Route("StandardLog")]
        public IHttpActionResult Log([FromBody]StandardLogModel logEntry)
        {
            string ipAdress = GetIpAddress();

            switch (logEntry.LogLevel)
            {
                case LogLevelEnum.Error:
                    StandardLogEventSource.Log.Error(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.StackTrace,
                        logEntry.OccurredAt,
                        logEntry.SessionId,
                        logEntry.UserName);
                    break;
                case LogLevelEnum.Warning:
                    StandardLogEventSource.Log.Warning(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.OccurredAt,
                        logEntry.SessionId,
                        logEntry.UserName);
                    break;
                case LogLevelEnum.Informational:
                    StandardLogEventSource.Log.Informational(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.OccurredAt,
                        logEntry.SessionId,
                        logEntry.UserName);
                    break;
                case LogLevelEnum.Verbose:
                    StandardLogEventSource.Log.Verbose(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.OccurredAt,
                        logEntry.SessionId,
                        logEntry.UserName);
                    break;
                case LogLevelEnum.Critical:
                    StandardLogEventSource.Log.Critical(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.StackTrace,
                        logEntry.OccurredAt,
                        logEntry.SessionId,
                        logEntry.UserName);
                    break;
                default:
                    break;
            }

            return Ok();
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <remarks>
        /// Sends a <paramref name="logEntry" /> coming from Blueprint Server Performance Logger to the logging service
        /// </remarks>
        /// <param name="logEntry">Log entry</param>
        /// <response code="200">OK.</response>
        [HttpPost]
        [Route("PerformanceLog")]
        public IHttpActionResult Log([FromBody]PerformanceLogModel logEntry)
        {
            string ipAdress = GetIpAddress();

            PerformanceLogEventSource.Log.Verbose(
                ipAdress,
                logEntry.Source,
                logEntry.Message,
                logEntry.OccurredAt,
                logEntry.SessionId,
                logEntry.UserName,
                logEntry.ThreadID,
                logEntry.ActionName,
                logEntry.CorrelationId,
                logEntry.Duration,
                logEntry.Namespace,
                logEntry.Class,
                logEntry.Test);

            return Ok();
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <remarks>
        /// Sends a <paramref name="logEntry" /> coming from Blueprint Server Performance Trace Logger to the logging service
        /// </remarks>
        /// <param name="logEntry">Log entry</param>
        /// <response code="200">OK.</response>
        [HttpPost]
        [Route("SQLTraceLog")]
        public IHttpActionResult Log([FromBody]SQLTraceLogModel logEntry)
        {
            string ipAdress = GetIpAddress();

            SQLTraceLogEventSource.Log.Verbose(
                ipAdress,
                logEntry.Source,
                logEntry.Message,
                logEntry.OccurredAt,
                logEntry.SessionId,
                logEntry.UserName,
                logEntry.ThreadID,
                logEntry.ActionName,
                logEntry.CorrelationId,
                logEntry.Duration,
                logEntry.Namespace,
                logEntry.Class,
                logEntry.Test,
                logEntry.TextData,
                logEntry.SPID,
                logEntry.Database);

            return Ok();
        }

        /// <summary>
        /// Get Log Entries
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">OK.</response>
        [HttpGet]
        [Route("GetLog")]
        [ResponseType(typeof(HttpResponseMessage))]
        [UnhandledExceptionFilter]
        public IHttpActionResult GetLog()
        {
            var limitRecords = I18NHelper.ToInt32Invariant(ConfigurationManager.AppSettings["LogRecordsLimit"]);
            var response = Request.CreateResponse(HttpStatusCode.OK);

            response.Content = new CsvLogContent().Generate(limitRecords,true);
            return ResponseMessage(response);
        }

        private string GetIpAddress()
        {
            string ipAdress = "";
            if (this.Request != null && this.Request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                if (ctx != null)
                {
                    ipAdress = ctx.Request.UserHostAddress;
                }
            }

            return ipAdress;
        }
    }
}
