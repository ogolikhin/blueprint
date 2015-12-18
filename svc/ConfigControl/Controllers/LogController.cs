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
                        logEntry.LineNumber,
                        logEntry.StackTrace);
                    break;
                case LogLevelEnum.Informational:
                    BlueprintEventSource.Log.Informational(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.MethodName,
                        logEntry.FilePath,
                        logEntry.LineNumber,
                        logEntry.StackTrace);
                    break;
                case LogLevelEnum.Verbose:
                    BlueprintEventSource.Log.Verbose(
                        ipAdress,
                        logEntry.Source,
                        logEntry.Message,
                        logEntry.MethodName,
                        logEntry.FilePath,
                        logEntry.LineNumber,
                        logEntry.StackTrace);
                    break;
                default:
                    break;
            }

            return Ok();
        }

    }

}
