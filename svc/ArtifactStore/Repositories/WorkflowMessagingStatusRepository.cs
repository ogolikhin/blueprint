using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using System.Collections.Generic;

using BluePrintSys.Messaging.CrossCutting.Helpers;
using ArtifactStore.Helpers;
using ServiceLibrary.Models.Workflow;
using System;
using ServiceLibrary.Models;
using BluePrintSys.Messaging.Models.Actions;

namespace ServiceLibrary.Repositories
{
    public class WorkflowMessagingStatusRepository : IStatusRepository
    {
        private readonly string _dbSchema;
        private readonly ISqlConnectionWrapper _connectionWrapper;
        public string Name { get; set; }

        public string AccessInfo { get; set; }

        public WorkflowMessagingStatusRepository(string cxn, string name, string dbSchema = ServiceConstants.DefaultDBSchema)
            : this(new SqlConnectionWrapper(cxn), cxn, name)
        {
            _dbSchema = dbSchema;
        }

        internal WorkflowMessagingStatusRepository(ISqlConnectionWrapper connectionWrapper, string accessInfo, string name)
        {
            _connectionWrapper = connectionWrapper;
            Name = name;
            AccessInfo = accessInfo;
        }
        public async Task<List<StatusResponse>> GetStatuses(int timeout)
        {
            return new List<StatusResponse> { await GetStatus(timeout) };
        }

        #pragma warning disable SA1028 // Code should not contain trailing whitespace
        private async Task<StatusResponse> GetStatus(int timeout)
        {
            // No use of "Timeout" at this point!

            var status = new StatusResponse()
            {
                Name = Name,
                AccessInfo = AccessInfo,
                Result = "true",
                NoErrors = true
            };
            // Try to send a Message to get the Messenger Status
            try
            {
                var tenant = (await _connectionWrapper.QueryAsync<TenantInfo>("[dbo].[GetTenantInfo]", 
                    commandType: CommandType.StoredProcedure)).FirstOrDefault();

                await WorkflowMessagingProcessor.Instance.SendMessageAsync(tenant.TenantId, new StatusCheckMessage());
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
