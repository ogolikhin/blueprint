﻿using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.StateTransition
{
    public class StateTransitionActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenant, ActionMessage actionMessage, IActionHandlerServiceRepository actionHandlerServiceRepository)
        {
            return await Task.FromResult(true);
        }
    }
}
