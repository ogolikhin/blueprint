using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Jobs;

namespace ActionHandlerService.MessageHandlers.GenerateDescendants
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
                || !message.DesiredArtifactTypeId.HasValue
                || string.IsNullOrWhiteSpace(message.UserName))
            {
                Log.Debug("Invalid GenerateDescendantsMessage received");
                return await Task.FromResult(true);
            }

            Logger.Log($"Handling of type: {message.ActionType} started for user ID {message.UserId}, revision ID {message.RevisionId} with message {message.ToJSON()}", message, tenant, LogLevel.Debug);

            var generateDescendantsInfo = new GenerateDescendantsInfo
            {
                RevisionId = message.RevisionId,
                ProjectId = message.ProjectId,
                ArtifacId = message.ArtifactId,
                UserId = message.UserId,
                ChildCount = message.ChildCount,
                DesiredArtifactTypeId = message.DesiredArtifactTypeId.Value,
                Predefined = (ItemTypePredefined)message.TypePredefined
            };

            var parameters = SerializationHelper.ToXml(generateDescendantsInfo);
            var sqlConnectionWrapper = new SqlConnectionWrapper(tenant.ConnectionString);
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
                message.ToString());

            if (job.HasValue)
            {
                Log.Debug($"Job scheduled for {message.ActionType} with id: {job.Value}");
            }

            return await Task.FromResult(true);
        }
    }
}
