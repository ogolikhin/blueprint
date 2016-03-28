using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Newtonsoft.Json;
using ServiceLibrary.Attributes;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly IStatusRepository AdminStatusRepo;
        internal readonly IStatusRepository RaptorStatusRepo;
        internal readonly IServiceLogRepository Log;

        public StatusController()
            : this(new SqlStatusRepository(WebApiConfig.AdminStorage, "GetStatus"), new SqlStatusRepository(WebApiConfig.RaptorMain, "GetStatus"), new ServiceLogRepository())
        {
        }

        internal StatusController(IStatusRepository adminStatusRepo, IStatusRepository raptorStatusRepo,  IServiceLogRepository log)
        {
            AdminStatusRepo = adminStatusRepo;
            RaptorStatusRepo = raptorStatusRepo;
            Log = log;
        }

        /// <summary>
        /// GetStatus
        /// </summary>
        /// <remarks>
        /// Returns the current status of AdminStore service.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        /// <response code="503">Service Unavailable.</response>
        [HttpGet, NoCache]
        [Route(""), NoSessionRequired]
        [ResponseType(typeof(ServiceStatus))]
        public async Task<IHttpActionResult> GetStatus()
        {
            var serviceStatus = new ServiceStatus();

            serviceStatus.AssemblyFileVersion = GetAssemblyFileVersion();

            try
            {
                var result = await AdminStatusRepo.GetStatus();
                if (result)
                {
                    return Ok(serviceStatus);
                }
                return new StatusCodeResult(HttpStatusCode.ServiceUnavailable, Request);
            }
            catch (Exception ex)
            {
                await Log.LogError(WebApiConfig.LogSourceStatus, ex);
                return InternalServerError();
            }

            //var response = Request.CreateResponse(HttpStatusCode.OK, getStatusResponse);
            //return ResponseMessage(response);
        }

        public static string GetAssemblyFileVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
        }
    }

    [JsonObject]
    public class ServiceStatus
    {
        [JsonProperty]
        public string AssemblyFileVersion;

        [JsonProperty]
        public Dictionary<string, string> DatabaseVersions;
    }
}
