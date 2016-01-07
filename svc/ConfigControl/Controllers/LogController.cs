using ServiceLibrary.EventSources;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Web;
using System.Web.Http;

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

        [HttpPost]
        [Route("")]
        public IHttpActionResult Log([FromBody]LogEntry logEntry)
        {
            var ipAdress = "";
            if (this.Request != null && this.Request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                if (ctx != null)
                {
                    ipAdress = ctx.Request.UserHostAddress;
                }
            }

            switch (logEntry.LogLevel)
            {
                case LogLevelEnum.Error:
                    BlueprintEventSource.Log.Error(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
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
                        logEntry.MethodName,
                        logEntry.FilePath,
                        logEntry.LineNumber);
                    break;
                case LogLevelEnum.Informational:
                    BlueprintEventSource.Log.Informational(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.MethodName,
                        logEntry.FilePath,
                        logEntry.LineNumber);
                    break;
                case LogLevelEnum.Verbose:
                    BlueprintEventSource.Log.Verbose(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.MethodName,
                        logEntry.FilePath,
                        logEntry.LineNumber);
                    break;
                default:
                    break;
            }

            return Ok();
        }

        [HttpPost]
        [Route("CLog")]
        public IHttpActionResult Log([FromBody]CLogEntry logEntry)
        {
            var ipAdress = "";
            if (this.Request != null && this.Request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                if (ctx != null)
                {
                    ipAdress = ctx.Request.UserHostAddress;
                }
            }

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
                            logEntry.OccuredAt,
                            logEntry.TimeZoneOffset,
                            logEntry.UserName);
                        break;
                    case LogLevelEnum.Warning:
                        CLogEventSource.Log.Warning(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccuredAt,
                            logEntry.TimeZoneOffset,
                            logEntry.UserName);
                        break;
                    case LogLevelEnum.Informational:
                        CLogEventSource.Log.Informational(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccuredAt,
                            logEntry.TimeZoneOffset,
                            logEntry.UserName);
                        break;
                    case LogLevelEnum.Verbose:
                        CLogEventSource.Log.Verbose(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccuredAt,
                            logEntry.TimeZoneOffset,
                            logEntry.UserName);
                        break;
                    case LogLevelEnum.Critical:
                        CLogEventSource.Log.Critical(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.StackTrace,
                            logEntry.OccuredAt,
                            logEntry.TimeZoneOffset,
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
                            logEntry.OccuredAt,
                            logEntry.TimeZoneOffset,
                            logEntry.UserName,
                            logEntry.ActionName,
                            logEntry.TotalDuration);
                        break;
                    case LogLevelEnum.Warning:
                        CLogEventSource.Log.WarningPerf(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccuredAt,
                            logEntry.TimeZoneOffset,
                            logEntry.UserName,
                            logEntry.ActionName,
                            logEntry.TotalDuration);
                        break;
                    case LogLevelEnum.Informational:
                        CLogEventSource.Log.InformationalPerf(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccuredAt,
                            logEntry.TimeZoneOffset,
                            logEntry.UserName,
                            logEntry.ActionName,
                            logEntry.TotalDuration);
                        break;
                    case LogLevelEnum.Verbose:
                        CLogEventSource.Log.VerbosePerf(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.OccuredAt,
                            logEntry.TimeZoneOffset,
                            logEntry.UserName,
                            logEntry.ActionName,
                            logEntry.TotalDuration);
                        break;
                    case LogLevelEnum.Critical:
                        CLogEventSource.Log.CriticalPerf(
                            ipAdress,
                            logEntry.Source,
                            logEntry.Message,
                            logEntry.StackTrace,
                            logEntry.OccuredAt,
                            logEntry.TimeZoneOffset,
                            logEntry.UserName,
                            logEntry.ActionName,
                            logEntry.TotalDuration);
                        break;
                    default:
                        break;
                }
            }

            return Ok();
        }

    }

}
