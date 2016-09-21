using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ConfigControl.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using System.Net.Http.Headers;

namespace ConfigControl.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly IStatusControllerHelper _statusControllerHelper;
        internal readonly string _preAuthorizedKey;

        public StatusController()
            : this(new StatusControllerHelper(
                        new List<IStatusRepository>
                        {
                            new SqlStatusRepository(SqlConfigRepository.AdminStorageDatabase, "AdminStorage")
                        },
                        "ConfigControl",
                        new ServiceLogRepository(),
                        WebApiConfig.LogSourceStatus
                    )             
                  )
        {
        }

        internal StatusController(IStatusControllerHelper scHelper)
        {
            _statusControllerHelper = scHelper;
            
        }

        /// <summary>
        /// GetStatus
        /// </summary>
        /// <remarks>
        /// Returns the current status of the service.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route(""), NoSessionRequired]
        [ResponseType(typeof(ServiceStatus))]
        public async Task<IHttpActionResult> GetStatus(string preAuthorizedKey = null)
        {
            ServiceStatus serviceStatus = await _statusControllerHelper.GetStatus();
            if (preAuthorizedKey == null)
            {
                serviceStatus = _statusControllerHelper.GetShorterStatus(serviceStatus);
            }
            if (serviceStatus.NoErrors)
            {
                return Ok(serviceStatus);
            }
            else
            {
                var response = Request.CreateResponse(HttpStatusCode.InternalServerError, serviceStatus);
                return ResponseMessage(response);
            }
        }

        /// <summary>
        /// GetStatusUpCheck
        /// </summary>
        /// <remarks>
        /// Returns 200 OK. Used to 'ping' the service.
        /// </remarks>
        /// <response code="200">OK.</response>
        [HttpGet, NoCache]
        [Route("upcheck"), NoSessionRequired]
        public IHttpActionResult GetStatusUpCheck()
        {
            return Ok();
        }
    }
}
