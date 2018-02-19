using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;

namespace BlueprintSys.RC.Services.MessageHandlers
{
    public static class ActionMessageSender
    {
        public static async Task Send(ActionMessage message, TenantInformation tenant, IWorkflowMessagingProcessor workflowMessagingProcessor)
        {
            Logger.Log($"Sending {message.ActionType} message with transaction ID '{message.TransactionId}' for tenant '{tenant.TenantId}': {message.ToJSON()}", message, tenant);
            await workflowMessagingProcessor.SendMessageAsync(tenant.TenantId, message);
            Logger.Log($"Successfully sent {message.ActionType} message", message, tenant);
        }
    }
}
