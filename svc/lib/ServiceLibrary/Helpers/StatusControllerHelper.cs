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
        Task<ShorterServiceStatus> GetShorterStatus();
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
            List<Task<StatusResponse>> statusResponses = new List<Task<StatusResponse>>();
            foreach (IStatusRepository statusRepo in StatusRepos)
            {
                statusResponses.Add(TryGetStatusResponse(statusRepo));
            }

            //Await the status check task results.
            serviceStatus.NoErrors = true;
            foreach (var result in statusResponses)
            {
                var statusResult = await result;
                serviceStatus.StatusResponses.Add(statusResult);

                serviceStatus.NoErrors &= statusResult.NoErrors;
            }

            return serviceStatus;
        }

        // For shorter version of status as per US955
        public async Task<ShorterServiceStatus> GetShorterStatus()
        {
            var shorterServiceStatus = new ShorterServiceStatus();

            shorterServiceStatus.ServiceName = ServiceName;

            //Get status responses from each repo, store the tasks.
            List<Task<ShorterStatusResponse>> shorterStatusResponses = new List<Task<ShorterStatusResponse>>();
            foreach (IStatusRepository statusRepo in StatusRepos)
            {
                shorterStatusResponses.Add(TryGetShorterStatusResponse(statusRepo));
            }

            //Await the status check task results.
            shorterServiceStatus.NoErrors = true;
            foreach (var result in shorterStatusResponses)
            {
                var statusResult = await result;
                shorterServiceStatus.ShorterStatusResponses.Add(statusResult);

                shorterServiceStatus.NoErrors &= statusResult.NoErrors;
            }

            return shorterServiceStatus;
        }
        

        /// <summary>
        /// Modifies serviceStatus in place, returns whether status was successfully obtained.
        /// </summary>
        private async Task<StatusResponse> TryGetStatusResponse(IStatusRepository statusRepo)
        {
            var responseData = new StatusResponse()
            {
                Name = statusRepo.Name,
                AccessInfo = statusRepo.AccessInfo
            };

            try
            {
                var result = await statusRepo.GetStatus(GET_STATUS_TIMEOUT);

                responseData.Result = result;
                responseData.NoErrors = true;
            }
            catch (Exception ex)
            {
                await Log.LogError(LogSource, ex);
                responseData.Result = $"ERROR: {ex.ToString()}";
                responseData.NoErrors = false;
            }

            return responseData;
        }

        // For shorter version of status as per US955
        private async Task<ShorterStatusResponse> TryGetShorterStatusResponse(IStatusRepository statusRepo)
        {
            var responseData = new ShorterStatusResponse()
            {
                Name = statusRepo.Name,
                
            };

            try
            {
                var result = await statusRepo.GetStatus(GET_STATUS_TIMEOUT);

                responseData.NoErrors = true;
            }
            catch (Exception ex)
            {
                await Log.LogError(LogSource, ex);
                responseData.NoErrors = false;
            }

            return responseData;
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

    // For shorter version of status as per US955
    public class ShorterServiceStatus
    {
        [JsonProperty]
        public string ServiceName;

        private List<ShorterStatusResponse> _shorterStatusResponses;

        [JsonProperty]
        public List<ShorterStatusResponse> ShorterStatusResponses
        {
            get
            {
                if (_shorterStatusResponses == null)
                {
                    _shorterStatusResponses = new List<ShorterStatusResponse>();
                }
                return _shorterStatusResponses;
            }
        }

        [JsonProperty]
        public bool NoErrors;

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

    // For shorter version of status as per US955

    public class ShorterStatusResponse
    {
        [JsonProperty]
        public string Name;

        [JsonProperty]
        public bool NoErrors;
    }


}
