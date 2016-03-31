using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using FileStore;
using FileStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace FileStore.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly StatusControllerHelper _statusControllerHelper;
        internal readonly string _preAuthorizedKey;

        public StatusController()
            : this(new StatusControllerHelper(
                        new List<IStatusRepository>
                        {
                            new SqlStatusRepository(ConfigRepository.Instance.FileStoreDatabase, "FileStorageDB"),
                            new ServiceDependencyStatusRepository(new Uri(WebApiConfig.AccessControl), "AccessControlEndpoint"),
                            new ServiceDependencyStatusRepository(new Uri(WebApiConfig.ConfigControl), "ConfigControlEndpoint")
                        },
                        "FileStore",
                        new ServiceLogRepository(),
                        WebApiConfig.LogSourceStatus), 
                        WebApiConfig.StatusCheckPreauthorizedKey
                  )
        {
        }

        internal StatusController(StatusControllerHelper scHelper, string preAuthorizedKey)
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
            if (_preAuthorizedKey == null || preAuthorizedKey != _preAuthorizedKey)
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized));
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