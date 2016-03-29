using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly StatusControllerHelper statusControllerHelper;

        public StatusController()
            : this( new StatusControllerHelper(
                        new List<IStatusRepository> {   new SqlStatusRepository(WebApiConfig.AdminStorage, "AdminStorage"),
                                                        new SqlStatusRepository(WebApiConfig.RaptorMain, "Raptor"),
                                                        new ServiceDependencyStatusRepository(new Uri("http://localhost:9801/svc/AdminStore/"), "AdminStore")},
                        new ServiceLogRepository(),
                        WebApiConfig.LogSourceStatus
                    )
                  )
        {
        }

        internal StatusController(StatusControllerHelper scHelper)
        {
            statusControllerHelper = scHelper;
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
        public async Task<IHttpActionResult> GetStatus()
        {
            ServiceStatus serviceStatus = await statusControllerHelper.GetStatus();

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
        [ResponseType(typeof(ServiceStatus))]
        public IHttpActionResult GetStatusUpCheck()
        {
            return Ok();
        }
    }
}
