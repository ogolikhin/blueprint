﻿using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Helpers;
using ServiceLibrary.LocalLog;
using ServiceLibrary.Repositories.ConfigControl;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactsPublished
{
    public class ArtifactsPublishedActionHelper : IActionHelper
    {
        private readonly IActionsParser _actionsParser;

        public ArtifactsPublishedActionHelper(IActionsParser actionsParser = null)
        {
            _actionsParser = actionsParser ?? new ActionsParser();
        }

        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            var message = (ArtifactsPublishedMessage)actionMessage;
            Logger.Log($"Handling started for user ID {message.UserId}, revision ID {message.RevisionId} with message {message.ToJSON()}", message, tenant, LogLevel.Debug);

            var repository = (IArtifactsPublishedRepository)baseRepository;

            var serviceLogRepository = new ServiceLogRepository(new HttpClientProvider(),
                new LocalFileLog(),
                tenant.AdminStoreLog);

            //Get modified properties for all artifacts and create a dictionary with key as artifact ids
            bool handledAllUpdatedArtifacts = await UpdatedArtifactsNotificationHandler.ProcessUpdatedArtifacts(tenant,
                message,
                repository,
                serviceLogRepository,
                _actionsParser,
                WorkflowMessagingProcessor.Instance);
            if (!handledAllUpdatedArtifacts)
            {
                Logger.Log("Could not process messages for all published updated artifacts", message, tenant, LogLevel.Debug);
            }
            
            var handledAllCreatedArtifacts =
                await
                    CreatedArtifactsNotificationHandler.ProcessCreatedArtifacts(tenant, 
                    message,
                    repository,
                    serviceLogRepository, 
                    WorkflowMessagingProcessor.Instance);

            if (!handledAllCreatedArtifacts)
            {
                Logger.Log("Could not process messages for all published created artifacts", message, tenant, LogLevel.Debug);
            }

            Logger.Log("Finished processing message", message, tenant, LogLevel.Debug);
            return handledAllUpdatedArtifacts && handledAllCreatedArtifacts;
        }
    }
}
