using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using AdminStore.Models;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [RoutePrefix("log")]
    public class LogController : ApiController
    {

        internal readonly IServiceLogRepository LogRepository;

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
            var sessionToken = Request.Headers.GetValues("Session-Token").FirstOrDefault();
            var sessionId = string.IsNullOrEmpty(sessionToken) ? "" : sessionToken.Substring(0, 8);
            Session session = (Session)ActionContext.Request.Properties[ServiceConstants.SessionProperty];
            string userName = session != null ? session.UserName : "";
           
            var result = await LogRepository.LogClientMessage(logEntry, sessionId, userName);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = result.Content;
            return ResponseMessage(response);
        }
    }
}