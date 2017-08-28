using System.Collections.Generic;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Jobs;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.Jobs;

namespace ActionHandlerService.MessageHandlers.GenerateTests
{
    //We should be creating specific action handlers for different  message handlers. 
    //These should be implemented when the actions are implemented
    public class GenerateTestsActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            var message = (GenerateTestsMessage) actionMessage;
            if (message == null 
                || message.ArtifactId <= 0 
                || message.ProjectId <= 0 
                || message.RevisionId <= 0
                || message.UserId <= 0
                || string.IsNullOrWhiteSpace(message.UserName))
            {
                Log.Debug("Invalid GenerateTestsMessage received");
                return await Task.FromResult(true);
            }

            Logger.Log($"Handling of type: {message.ActionType} started for user ID {message.UserId}, revision ID {message.RevisionId} with message {message.ToJSON()}", message, tenant, LogLevel.Debug);
            
            var generateProcessTestInfos = new List<GenerateProcessTestInfo>
            {
                new GenerateProcessTestInfo
                {
                    ProcessId = message.ArtifactId
                }
            };
            var parameters = SerializationHelper.ToXml(generateProcessTestInfos);
            var sqlConnectionWrapper = new SqlConnectionWrapper(tenant.ConnectionString);
            var jobsRepository = new JobsRepository(sqlConnectionWrapper, 
                new SqlArtifactRepository(sqlConnectionWrapper), 
                new SqlArtifactPermissionsRepository(sqlConnectionWrapper),
                new SqlUsersRepository(sqlConnectionWrapper));

            var job = await jobsRepository.AddJobMessage(JobType.GenerateProcessTests,
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
            
            return await Task.FromResult(true);
        }
    }
}
