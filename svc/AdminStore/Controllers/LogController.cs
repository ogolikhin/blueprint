using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [BaseExceptionFilter]
    [RoutePrefix("log")]
    public class LogController : ApiController
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

            var sessionToken = Request.Headers.GetValues("Session-Token").FirstOrDefault();
            var sessionId = string.IsNullOrEmpty(sessionToken) ? "" : sessionToken.Substring(0, 8);
            string userName = GetUserName();

            var result = await LogRepository.LogClientMessage(logEntry, sessionId, userName);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = result.Content;

            return ResponseMessage(response);
        }

        private string GetUserName()
        {
            object sessionObject = null;
            if (ActionContext.Request.Properties.TryGetValue(ServiceConstants.SessionProperty, out sessionObject))
            {
                Session session = sessionObject as Session;
                if (session != null)
                {
                    return session.UserName;
                }
            }

            return string.Empty;
        }
    }
}
