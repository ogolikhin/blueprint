using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Management;
using Newtonsoft.Json;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace ServiceLibrary.Helpers
{
    public class StatusControllerHelper
    {
        private readonly List<IStatusRepository> StatusRepos;
        private readonly IServiceLogRepository Log;
        private readonly string LogSource;

        public StatusControllerHelper(List<IStatusRepository> statusRepos, IServiceLogRepository log, string logSource)
        {
            StatusRepos = statusRepos;
            Log = log;
            LogSource = logSource;
        }

        public async Task<ServiceStatus> GetStatus()
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
            serviceStatus.NoErrors = true;
            foreach (var result in StatusResults)
            {
                serviceStatus.NoErrors &= await result;
            }

            return serviceStatus;
        }

        /// <summary>
        /// Modifies serviceStatus in place, returns whether status was successfully obtained.
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
                await Log.LogError(LogSource, ex);
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

    // *************************************************************************************
    // ***** Any changes to this class needs to be replicated in the                   *****
    // ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
    // *************************************************************************************
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

        [JsonProperty]
        public bool NoErrors;
    }
}
