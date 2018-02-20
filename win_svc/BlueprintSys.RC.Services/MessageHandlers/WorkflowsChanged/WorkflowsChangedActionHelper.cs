using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.WorkflowsChanged
{
    public class WorkflowsChangedActionHelper : MessageActionHandler
    {
        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            return await HandleWorkflowsChangedAction(tenant, actionMessage, baseRepository, WorkflowMessagingProcessor.Instance);
        }

        public async Task<bool> HandleWorkflowsChangedAction(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository, IWorkflowMessagingProcessor workflowMessagingProcessor)
        {
            var message = (WorkflowsChangedMessage) actionMessage;
            var repository = (IWorkflowsChangedRepository) baseRepository;

            Logger.Log("Getting affected artifact IDs", message, tenant);
            var workflowIds = new[]
            {
                message.WorkflowId
            };
            var artifactIds = await repository.GetAffectedArtifactIds(workflowIds, message.RevisionId);
            await ArtifactsChangedMessageSender.Send(artifactIds, tenant, actionMessage, workflowMessagingProcessor);
            return true;
        }
    }
}
