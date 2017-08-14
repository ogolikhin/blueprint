using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("log")]
    public class LogController : BaseApiController
    {
        private readonly IServiceLogRepository LogRepository;

        public LogController() : this(new ServiceLogRepository())
        {
        }

        internal LogController(IServiceLogRepository log)
        {
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
        [Route(""), SessionOptional]
        public async Task<IHttpActionResult> Log([FromBody]ClientLogModel logEntry)
        {
            if (logEntry == null)
            {
                return BadRequest("Log entry not provided or malformed");
            }

            var sessionId = ExtractSessionId(Request.Headers);
            string userName = GetUserName();

            var result = await LogRepository.LogClientMessage(logEntry, sessionId, userName);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = result.Content;

            return ResponseMessage(response);
        }

        internal static string ExtractSessionId(HttpHeaders headers)
        {
            IEnumerable<string> values = null;
            if (headers == null || !headers.TryGetValues(ServiceConstants.BlueprintSessionTokenKey, out values))
            {
                return string.Empty;
            }

            var sessionToken = values.FirstOrDefault();
            if (string.IsNullOrEmpty(sessionToken))
            {
                return string.Empty;
            }

            return sessionToken.Substring(0, Math.Min(8, sessionToken.Length));
        }

        private string GetUserName()
        {
            return Session?.UserName ?? string.Empty;
        }
    }
}
