﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Models;
using BlueprintSys.RC.Services.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Models.Enums;

namespace BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged
{
    public class ArtifactsChangedActionHelper : MessageActionHandler
    {
        protected override Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage,
            IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            if (!actionMessage.ActionType.HasFlag(MessageActionType.ArtifactsChanged))
            {
                throw new NotSupportedException("Artifacts changed handler can only handle Artifacts Changed type");
            }
            var artifactsChangedMessage = actionMessage as ArtifactsChangedMessage;
            Log.DebugFormat("ArtifactsChanged message received for artifact ids {0}.", string.Join(",", artifactsChangedMessage.ArtifactIds.Select(a => a.ToString()).ToArray()));
            // call service repostiory to execute stored procedure.
            return Task.FromResult(true);
        }
    }
}
