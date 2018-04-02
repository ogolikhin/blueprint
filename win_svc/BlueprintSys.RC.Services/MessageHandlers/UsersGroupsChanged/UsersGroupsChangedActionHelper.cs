using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers.UsersGroupsChanged
{
    public class UsersGroupsChangedActionHelper : MessageActionHandler
    {
        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            return await HandleUsersGroupsChangedAction(tenant, actionMessage, baseRepository, WorkflowMessagingProcessor.Instance);
        }

        public async Task<bool> HandleUsersGroupsChangedAction(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository, IWorkflowMessagingProcessor workflowMessagingProcessor)
        {
            var message = (UsersGroupsChangedMessage)actionMessage;
            var repository = (IUsersGroupsChangedRepository)baseRepository;

            Logger.Log($"Handling Users Groups Changed Message for change type {message.ChangeType}", message, tenant);

            if (message.ChangeType == UsersGroupsChangedType.Create)
            {
                Logger.Log("No need to send Artifacts Changed Messages when Creating Users or Groups", message, tenant);
                return true;
            }

            Logger.Log("Getting affected artifact IDs", message, tenant);
            var artifactIds = await repository.GetAffectedArtifactIds(message.UserIds, message.GroupIds, message.RevisionId);
            await ArtifactsChangedMessageSender.Send(artifactIds, tenant, actionMessage, workflowMessagingProcessor, repository);
            return true;
        }
    }
}
