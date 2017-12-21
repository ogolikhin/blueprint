using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;

namespace BlueprintSys.RC.Services.MessageHandlers.GenerateDescendants
{
    public class GenerateDescendantsActionHelper : BoundaryReachedActionHandler
    {
        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            var message = (GenerateDescendantsMessage)actionMessage;
            if (message == null || message.ArtifactId <= 0 || message.ProjectId <= 0 || message.RevisionId <= 0 || message.UserId <= 0 || message.DesiredArtifactTypeId == null || string.IsNullOrWhiteSpace(message.UserName) || tenant == null)
            {
                Logger.Log($"Invalid GenerateDescendantsMessage received: {message?.ToJSON()}", message, tenant, LogLevel.Error);
                return false;
            }

            Logger.Log($"Handling of type: {message.ActionType} started for user ID {message.UserId}, revision ID {message.RevisionId} with message {message.ToJSON()}", message, tenant, LogLevel.Debug);

            var ancestors = new List<int>(message.AncestorArtifactTypeIds ?? new int[0]);
            Logger.Log($"{ancestors.Count} ancestor artifact type IDs found: {string.Join(",", ancestors)}", message, tenant, LogLevel.Debug);
            var ancestorLoopExists = ancestors.GroupBy(i => i).Any(group => group.Count() > 1);
            if (ancestorLoopExists)
            {
                Logger.Log("Child generation was stopped. Infinite loop detected in the ancestor artifact type IDs.", message, tenant, LogLevel.Debug);
                return false;
            }

            var repository = (IGenerateActionsRepository)baseRepository;
            var sqlItemTypeRepository = repository.ItemTypeRepository;
            var desiredItemType = await sqlItemTypeRepository.GetCustomItemTypeForProvidedStandardItemTypeIdInProject(message.ProjectId, message.DesiredArtifactTypeId.GetValueOrDefault());
            if (desiredItemType == null || desiredItemType.ItemTypeId <= 0 || string.IsNullOrWhiteSpace(desiredItemType.Name))
            {
                Logger.Log($"No artifact type found with instance Id: {message.DesiredArtifactTypeId.GetValueOrDefault()} in Project: {message.ProjectId}", message, tenant);
                return false;
            }

            var generateDescendantsInfo = new GenerateDescendantsInfo
            {
                RevisionId = message.RevisionId,
                ProjectId = message.ProjectId,
                ArtifactId = message.ArtifactId,
                UserId = message.UserId,
                ChildCount = message.ChildCount,
                DesiredArtifactTypeId = desiredItemType.ItemTypeId,
                Predefined = (ItemTypePredefined)message.TypePredefined,
                DesiredArtifactTypeName = desiredItemType.Name,
                AncestorArtifactTypeIds = message.AncestorArtifactTypeIds?.ToArray()
            };
            
            var parameters = SerializationHelper.ToXml(generateDescendantsInfo);

            var user = await repository.GetUser(message.UserId);
            Logger.Log($"Retrieved user: {user?.Login}", message, tenant);

            var jobId = await repository.JobsRepository.AddJobMessage(JobType.GenerateDescendants,
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
                Logger.Log($"Job scheduled for {message.ActionType} with id: {jobId.Value}", message, tenant);
            }
            else
            {
                Logger.Log("No jobId received", message, tenant);
            }

            return jobId.HasValue && jobId > 0;
        }
    }
}
