using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;

namespace BlueprintSys.RC.Services.MessageHandlers.GenerateDescendants
{
    public class GenerateDescendantsActionHelper : BoundaryReachedActionHandler
    {
        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            var message = (GenerateDescendantsMessage)actionMessage;
            if (message == null
                || message.ArtifactId <= 0
                || message.ProjectId <= 0
                || message.RevisionId <= 0
                || message.UserId <= 0
                || message.DesiredArtifactTypeId == null
                || string.IsNullOrWhiteSpace(message.UserName))
            {
                Log.Debug("Invalid GenerateDescendantsMessage received");
                return false;
            }

            Logger.Log($"Handling of type: {message.ActionType} started for user ID {message.UserId}, revision ID {message.RevisionId} with message {message.ToJSON()}", message, tenant, LogLevel.Debug);

            var genDescendantsActionHelper = (IGenerateActionsRepository)actionHandlerServiceRepository;

            var sqlItemTypeRepository = genDescendantsActionHelper.ItemTypeRepository;

            var desiredItemType = await sqlItemTypeRepository.GetCustomItemTypeForProvidedStandardItemTypeIdInProject(message.ProjectId,
                message.DesiredArtifactTypeId.GetValueOrDefault());
            if (desiredItemType == null || desiredItemType.ItemTypeId <= 0 || string.IsNullOrWhiteSpace(desiredItemType.Name))
            {
                Log.Debug($"No artifact type found with instance Id: {message.DesiredArtifactTypeId.GetValueOrDefault()} in Project: {message.ProjectId}");
                return false;
            }

            var generateDescendantsInfo = new GenerateDescendantsInfo
            {
                RevisionId = message.RevisionId,
                ProjectId = message.ProjectId,
                ArtifacId = message.ArtifactId,
                UserId = message.UserId,
                ChildCount = message.ChildCount,
                DesiredArtifactTypeId = desiredItemType.ItemTypeId,
                Predefined = (ItemTypePredefined)message.TypePredefined,
                DesiredArtifactTypeName = desiredItemType.Name
            };
            
            var parameters = SerializationHelper.ToXml(generateDescendantsInfo);
            
            var user = await GetUserInfo(message, actionHandlerServiceRepository);

            var jobId = await genDescendantsActionHelper.JobsRepository.AddJobMessage(JobType.GenerateDescendants,
                false,
                parameters,
                null,
                message.ProjectId,
                message.ProjectName,
                message.UserId,
                user?.Login, //Login is equal to username
                message.BaseHostUri);

            if (jobId.HasValue)
            {
                Log.Debug($"Job scheduled for {message.ActionType} with id: {jobId.Value}");
            }

            return jobId.HasValue && jobId > 0;
        }
    }
}
