using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Jobs;

namespace BlueprintSys.RC.Services.MessageHandlers.GenerateDescendants
{
    public class GenerateDescendantsActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IActionHandlerServiceRepository actionHandlerServiceRepository)
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

            var sqlConnectionWrapper = new SqlConnectionWrapper(tenant.BlueprintConnectionString);
            var sqlItemTypeRepository = new SqlItemTypeRepository(sqlConnectionWrapper);

            var desiredItemType = await sqlItemTypeRepository.GetCustomItemTypeForProvidedStandardItemTypeIdInProject(message.ProjectId,
                message.DesiredArtifactTypeId.GetValueOrDefault());
            if (desiredItemType == null || desiredItemType.ItemTypeId <= 0)
            {
                Log.Debug($"No artifact type found with instance Id: {message.DesiredArtifactTypeId.Value} in Project: {message.ProjectId}");
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
                Predefined = (ItemTypePredefined)message.TypePredefined
            };

            var parameters = SerializationHelper.ToXml(generateDescendantsInfo);

            var jobsRepository = new JobsRepository(sqlConnectionWrapper,
                new SqlArtifactRepository(sqlConnectionWrapper),
                new SqlArtifactPermissionsRepository(sqlConnectionWrapper),
                new SqlUsersRepository(sqlConnectionWrapper));

            var job = await jobsRepository.AddJobMessage(JobType.GenerateDescendants,
                false,
                parameters,
                null,
                message.ProjectId,
                message.ProjectName,
                message.UserId,
                message.UserName,
                message.BaseHostUri);

            if (job.HasValue)
            {
                Log.Debug($"Job scheduled for {message.ActionType} with id: {job.Value}");
            }

            return true;
        }
    }
}
