using System;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using System.Collections.Generic;
using BluePrintSys.Messaging.CrossCutting.Helpers;


namespace ServiceLibrary.Repositories
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
            // No use of "Timeout" at this point!

            var status = new StatusResponse()
            {
                Name = Name,
                AccessInfo = string.IsNullOrEmpty(AccessInfo) ? "false" : "true",
                Result = "true",
                NoErrors = true
            };
            // Try to send a Message to get the Messenger Status
            try
            {
                await WorkflowMessagingProcessor.Instance.CheckStatusAsync();
            }
            catch (Exception ex)
            {
                // Log the exception later if it wasn't already logged
                string msg = ex.Message;
                status.Result = "false";
                status.NoErrors = false;
            }
            return status;
        }
    }
}
