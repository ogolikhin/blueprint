using System;
using System.Data;
using System.Threading.Tasks;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace BluePrintSys.Messaging.CrossCutting.Helpers
{
    public interface ISendMessageExecutor
    {
        Task Execute(IApplicationSettingsRepository applicationSettingsRepository, IServiceLogRepository serviceLogRepository, ActionMessage message, IDbTransaction transaction = null);
    }

    public class SendMessageExecutor : ISendMessageExecutor
    {
        public async Task Execute(IApplicationSettingsRepository applicationSettingsRepository, IServiceLogRepository serviceLogRepository, ActionMessage message, IDbTransaction transaction = null)
        {
            var tenantInfo = await applicationSettingsRepository.GetTenantInfo(transaction);
            var tenantId = tenantInfo?.TenantId;
            try
            {
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    throw new TenantInfoNotFoundException("No tenant information found. Please contact your administrator.");
                }
                await WorkflowMessagingProcessor.Instance.SendMessageAsync(tenantId, message);
                await serviceLogRepository.LogInformation("SendMessageExecutor", $"Sent {message.ActionType} message for tenant {tenantId}: {message.ToJSON()}");
            }
            catch (Exception ex)
            {
                await serviceLogRepository.LogError("SendMessageExecutor", $"Failed to send {message.ActionType} message for tenant {tenantId}: {message.ToJSON()}. Exception: {ex.Message}");
                throw;
            }
        }
    }
}
