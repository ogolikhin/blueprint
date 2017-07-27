﻿using ActionHandlerService.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.ArtifactPublished
{
    public class ArtifactsPublishedMessageHandler : BaseMessageHandler<ArtifactsPublishedMessage>
    {
        public ArtifactsPublishedMessageHandler() : this(new ArtifactsPublishedActionHelper(), new TenantInfoRetriever(), new ConfigHelper())
        {
        }

        public ArtifactsPublishedMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper) : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
