using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
 

namespace ServiceLibrary.Helpers
{
    public interface IStatusControllerHelper
    {
        Task<ServiceStatus> GetStatus();
         ServiceStatus GetShorterStatus(ServiceStatus s);

    }

    public class StatusControllerHelper : IStatusControllerHelper
    {
        private const int GET_STATUS_TIMEOUT = 50;

        private readonly List<IStatusRepository> StatusRepos;
        private readonly string ServiceName;
        private readonly IServiceLogRepository Log;
        private readonly string LogSource;

        public StatusControllerHelper(List<IStatusRepository> statusRepos, string serviceName, IServiceLogRepository log, string logSource)
        {
            StatusRepos = statusRepos;
            ServiceName = serviceName;
            Log = log;
            LogSource = logSource;
        }

        public async Task<ServiceStatus> GetStatus()
        {
            var serviceStatus = new ServiceStatus();

            serviceStatus.ServiceName = ServiceName;
            serviceStatus.AssemblyFileVersion = GetAssemblyFileVersion();

            //Get status responses from each repo, store the tasks.
            List<Task<List<StatusResponse>>> statusResponses = new List<Task<List<StatusResponse>>>();
            foreach (IStatusRepository statusRepo in StatusRepos)
            {
                Task<List<StatusResponse>> response = TryGetStatusResponse(statusRepo);
                statusResponses.Add(response);
            }

            //Await the status check task results.
            serviceStatus.NoErrors = true;
            foreach (var result in statusResponses)
            {
                List<StatusResponse> statusResult = await result;
                serviceStatus.StatusResponses.AddRange(statusResult);

                statusResult.ForEach((response) => { serviceStatus.NoErrors &= response.NoErrors; });
                
            }

            return serviceStatus;
        }

        public  ServiceStatus GetShorterStatus(ServiceStatus s)
        {
            var serviceStatus = new ServiceStatus();

            serviceStatus.ServiceName = s.ServiceName;

            List<StatusResponse> statusResponses = new List<StatusResponse>();

            if (s.StatusResponses.Count > 0)
            {
                
                foreach (var statusResponse in s.StatusResponses)
                {
                    statusResponses.Add(new StatusResponse { Name = statusResponse.Name, NoErrors = statusResponse.NoErrors });
                }
            }

            
            serviceStatus.NoErrors = true;
            foreach (var result in statusResponses)
            {
                var statusResult = result;
                serviceStatus.StatusResponses.Add(statusResult);
                serviceStatus.NoErrors = s.NoErrors;
            }
            
            return serviceStatus;
            
        }

        /// <summary>
        /// Modifies serviceStatus in place, returns whether status was successfully obtained.
        /// </summary>
        private async Task<List<StatusResponse>> TryGetStatusResponse(IStatusRepository statusRepo)
        {
            
            try
            {
                return await statusRepo.GetStatuses(GET_STATUS_TIMEOUT);
               
            }
            catch (Exception ex)
            {
                List<StatusResponse> r = new List<StatusResponse>();
                var responseData = new StatusResponse()
                {
                    Name = statusRepo.Name,
                    AccessInfo = statusRepo.AccessInfo,
                    Result = $"ERROR: {ex.ToString()}",
                    NoErrors = false
                };
                r.Add(responseData);
                 return r;
            }

           
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
        public string ServiceName;

        [JsonProperty]
        public string AccessInfo;

        [JsonProperty]
        public string AssemblyFileVersion;

        private List<StatusResponse> _statusResponses;

        [JsonProperty]
        public List<StatusResponse> StatusResponses
        {
            get
            {
                if (_statusResponses == null)
                {
                    _statusResponses = new List<StatusResponse>();
                }
                return _statusResponses;
            }
        }

        [JsonProperty]
        public bool NoErrors;

        [JsonProperty]
        public string Errors;
    }

    public class StatusResponse
    {
        [JsonProperty]
        public string Name;

        [JsonProperty]
        public string AccessInfo;

        [JsonProperty]
        public string Result;

        [JsonProperty]
        public bool NoErrors;
    }
}
