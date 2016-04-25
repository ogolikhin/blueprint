using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using AdminStore.Models;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [RoutePrefix("log")]
    public class LogController : ApiController
    {

        internal readonly IHttpClientProvider HttpClientProvider;
        internal readonly IServiceLogRepository LogRepository;

        public LogController() : this(new HttpClientProvider(), new ServiceLogRepository())
        {

        }

        internal LogController(IHttpClientProvider httpClientProvider, IServiceLogRepository log)
        {
            HttpClientProvider = httpClientProvider;
            LogRepository = log;
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <remarks>
        /// Sends a <paramref name="logEntry" /> coming from the nova client to the logging service
        /// </remarks>
        /// <param name="logEntry">Log entry</param>
        /// <response code="200">OK.</response>
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Log([FromBody]ClientLogModel logEntry)
        {
            try
            {
                ServiceLogModel serviceLog = new ServiceLogModel
                {
                    Source = logEntry.Source,
                    LogLevel = (LogLevelEnum)logEntry.LogLevel,
                    Message = logEntry.Message,
                    OccurredAt = DateTime.Now,
                    SessionId = logEntry.SessionId,
                    UserName = logEntry.UserName,
                    MethodName = logEntry.MethodName,
                    FilePath = logEntry.FilePath,
                    LineNumber = logEntry.LineNumber,
                    StackTrace = logEntry.StackTrace
                };

                var uri = new Uri(WebApiConfig.ConfigControl);
                var http = HttpClientProvider.Create(uri);
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri, "log"),
                    Method = HttpMethod.Post,
                    Content = new ObjectContent(typeof (ServiceLogModel), serviceLog, new JsonMediaTypeFormatter())
                };
                request.Headers.Add("Session-Token", Request.Headers.GetValues("Session-Token").FirstOrDefault());
                var result = await http.SendAsync(request);
                result.EnsureSuccessStatusCode();
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = result.Content;
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                await LogRepository.LogError(WebApiConfig.LogSourceConfig, ex);
                return InternalServerError();
            }
        }
    }
}