﻿using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using ConfigControl.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace ConfigControl.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly IStatusRepository StatusRepo;
        internal readonly IServiceLogRepository Log;

        public StatusController()
            : this(new SqlStatusRepository(SqlConfigRepository.AdminStorageDatabase, "GetStatus", "AdminStorage"), new ServiceLogRepository())
        {
        }

        internal StatusController(IStatusRepository statusRepo, IServiceLogRepository log)
        {
            StatusRepo = statusRepo;
            Log = log;
        }

        /// <summary>
        /// GetStatus
        /// </summary>
        /// <remarks>
        /// Returns the current status of ConfigControl service.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <response code="503">Service Unavailable.</response>
        [HttpGet, NoCache]
        [Route("")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> GetStatus()
        {
            var result = await StatusRepo.GetStatus();
            return Ok();
            /*
            try
            {
                var result = await StatusRepo.GetStatus();
                if (result)
                {
                    return Ok();
                }
                return new StatusCodeResult(HttpStatusCode.ServiceUnavailable, Request);
            }
            catch (Exception ex)
            {
                await Log.LogError(WebApiConfig.LogSourceStatus, ex);
                return InternalServerError();
            }*/
        }
    }
}
