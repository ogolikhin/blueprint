﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using ServiceLibrary.Attributes;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly IStatusRepository _statusRepo;

        public StatusController() : this(new SqlStatusRepository(WebApiConfig.ArtifactStorage, "GetStatus"))
        {
        }

        internal StatusController(IStatusRepository statusRepo)
        {
            _statusRepo = statusRepo;
        }

        /// <summary>
        /// GetStatus
        /// </summary>
        /// <remarks>
        /// Returns the current status of ArtifactStore service.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <response code="503">Service Unavailable.</response>
        [HttpGet, NoCache]
        [Route(""), NoSessionRequired]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> GetStatus()
        {
            try
            {
                var result = await _statusRepo.GetStatus();
                if (result)
                {
                    return Ok();
                }
                return new StatusCodeResult(HttpStatusCode.ServiceUnavailable, Request);
            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
