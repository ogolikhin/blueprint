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
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [RoutePrefix("status")]
    public class StatusController : ApiController
    {
        internal readonly List<IStatusRepository> StatusRepos;

        internal readonly IStatusRepository AdminStatusRepo;
        internal readonly IStatusRepository RaptorStatusRepo;
        internal readonly IServiceLogRepository Log;

        public StatusController()
            : this( new List<IStatusRepository> { new SqlStatusRepository(WebApiConfig.AdminStorage, "GetStatus", "AdminStorage"),
                                                  new SqlStatusRepository(WebApiConfig.RaptorMain, "GetStatus", "Raptor"),
                                                  new ServiceDependencyStatusRepository(new HttpClientProvider(), new Uri("http://localhost:9801/svc/AdminStore/"), "AdminStore")},
                    new ServiceLogRepository())
        {
        }

        internal StatusController(List<IStatusRepository> statusRepos, IServiceLogRepository log)
        {
            StatusRepos = statusRepos;
            Log = log;
        }

        /// <summary>
        /// GetStatus
        /// </summary>
        /// <remarks>
        /// Returns the current status of the service.
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

            //Get status responses from each repo, store the tasks.
            List<Task<bool>> StatusResults = new List<Task<bool>>();
            foreach (IStatusRepository statusRepo in StatusRepos)
            {
                StatusResults.Add(TryGetStatusResponse(serviceStatus, statusRepo));
            }

            //Await the status check task results.
            bool success = true;
            foreach (var result in StatusResults)
            {
                success &= await result;
            }

            if (success)
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

        /// <summary>
        /// Modifies serviceStatus in place, returns whether DatabaseVersion was successfully obtained.
        /// </summary>
        private async Task<bool> TryGetStatusResponse(ServiceStatus serviceStatus, IStatusRepository statusRepo)
        {
            try
            {
                var result = await statusRepo.GetStatus();
                serviceStatus.StatusResponses[statusRepo.Name] = result;
            }
            catch (Exception ex)
            {
                await Log.LogError(WebApiConfig.LogSourceStatus, ex);
                serviceStatus.StatusResponses[statusRepo.Name] = "ERROR";
                return false;
            }

            return true;
        }

        private static string GetAssemblyFileVersion()
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

        private Dictionary<string, string> _statusResponses;

        [JsonProperty]
        public Dictionary<string, string> StatusResponses
        {
            get
            {
                if (_statusResponses == null)
                {
                    _statusResponses = new Dictionary<string, string>();
                }
                return _statusResponses;
            }
        }
    }
}
