using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Logging;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories.ConfigControl;

namespace BluePrintSys.Messaging.CrossCutting.Helpers
{

    public interface ISendMessageExecutor
    {
        Task Execute(IWorkflowMessage message, SendMessageAsyncDelegate action, string actionMessage, string tenantId);
    }

    public class SendMessageExecutor : ISendMessageExecutor
    {
        private readonly IServiceLogRepository _serviceLogRepository;
        private readonly string _logSource;
        public SendMessageExecutor(IServiceLogRepository serviceLogRepository, string logSource)
        {
            _serviceLogRepository = serviceLogRepository;
            _logSource = logSource;
        }
        public async Task Execute(IWorkflowMessage workflowMessage, SendMessageAsyncDelegate action, string actionMessage, string tenantId)
        {
            try
            {
                await action(tenantId, workflowMessage);
                string message = $"Sent {workflowMessage.ActionType} message: {actionMessage.ToJSON()} with tenant id: {tenantId} to the Message queue";
                await _serviceLogRepository.LogInformation(_logSource, message);
            }
            catch (Exception ex)
            {
                await _serviceLogRepository.LogError(_logSource, $"Workflow messaging failed to send message with following exception: {ex}.");
                throw;
            }
        }
    }
}
