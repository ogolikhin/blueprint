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
using System.Net.Http.Headers;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly IStatusControllerHelper _statusControllerHelper;
        internal readonly string _preAuthorizedKey;

        public StatusController()
            : this(new StatusControllerHelper(
                        new List<IStatusRepository> {   new SqlStatusRepository(WebApiConfig.AdminStorage, "AdminStorageDB"),
                                                        new SqlStatusRepository(ServiceConstants.RaptorMain, "RaptorDB"),
                                                        new ServiceDependencyStatusRepository(new Uri(WebApiConfig.AccessControl), "AccessControlEndpoint"),
                                                        new ServiceDependencyStatusRepository(new Uri(WebApiConfig.ConfigControl), "ConfigControlEndpoint")},
                        "AdminStore",
                        new ServiceLogRepository(),
                        WebApiConfig.LogSourceStatus
                    ), WebApiConfig.StatusCheckPreauthorizedKey
                  )
        {
        }

        internal StatusController(IStatusControllerHelper scHelper, string preAuthorizedKey)
        {
            _statusControllerHelper = scHelper;
            _preAuthorizedKey = preAuthorizedKey;
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
            //Check pre-authorized key
            // Refactoring for shorter status as per US955
            if (preAuthorizedKey == null)
            {
                //ShorterServiceStatus shorterServiceStatus = await _statusControllerHelper.GetShorterStatus();
                ServiceStatus serviceStatus = await _statusControllerHelper.GetStatus();
                serviceStatus = _statusControllerHelper.GetShorterStatus(serviceStatus);

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
            else {
                if (preAuthorizedKey != _preAuthorizedKey)
                {
                    var unauthorizedMessage = "Unauthorized";
                    var shorterResponseMedia = new MediaTypeHeaderValue("application/json");
                    var response = Request.CreateResponse(HttpStatusCode.Unauthorized, unauthorizedMessage, shorterResponseMedia);
                    return ResponseMessage(response);
                }

                ServiceStatus serviceStatus = await _statusControllerHelper.GetStatus();

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