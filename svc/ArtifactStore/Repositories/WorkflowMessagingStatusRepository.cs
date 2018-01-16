using System;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using System.Collections.Generic;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories
{
    public class WorkflowMessagingStatusRepository : IStatusRepository
    {
        public string Name { get; set; }

        public string AccessInfo { get; set; }

        public WorkflowMessagingStatusRepository(string name)
        {
            Name = name;
        }

        public async Task<List<StatusResponse>> GetStatuses(int timeout)
        {
            return new List<StatusResponse> { await GetStatus(timeout) };
        }

        private async Task<StatusResponse> GetStatus(int timeout)
        {
            var status = new StatusResponse()
            {
                Name = Name,
                Result = "true",
                NoErrors = true
            };

            try
            {
                await WorkflowMessagingProcessor.Instance.GetStatusAsync(timeout);
            }
            catch (Exception ex)
            {
                status.Result = $"ERROR: {ex.Message}";
                status.NoErrors = false;
            }
            return status;
        }
    }
}
