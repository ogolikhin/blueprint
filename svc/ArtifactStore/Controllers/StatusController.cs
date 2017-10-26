﻿using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace ArtifactStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        private readonly IStatusControllerHelper _statusControllerHelper;
        private readonly string _expectedPreAuthorizedKey;

        public StatusController()
            : this(new StatusControllerHelper(
                        new List<IStatusRepository> { /* new SqlStatusRepository(WebApiConfig.ArtifactStorage, "ArtifactStorage"), //ArtifactStorage db is currently unused */
                                                        new SqlStatusRepository(ServiceConstants.RaptorMain, "RaptorDB") },
                        "ArtifactStore",
                        new ServiceLogRepository(),
                        WebApiConfig.LogSourceStatus),
                    WebApiConfig.StatusCheckPreauthorizedKey)
        {
        }

        internal StatusController(IStatusControllerHelper scHelper, string preAuthorizedKey)
        {
            _statusControllerHelper = scHelper;
            _expectedPreAuthorizedKey = preAuthorizedKey;
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
            // Check pre-authorized key
            // Refactoring for shorter status as per US955

            if (preAuthorizedKey != null && preAuthorizedKey != _expectedPreAuthorizedKey)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, "Unauthorized", new MediaTypeHeaderValue("application/json")));

            }

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
