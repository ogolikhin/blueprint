﻿using System.Threading.Tasks;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.GenerateTests
{
    //We should be creating specific action handlers for different  message handlers. 
    //These should be implemented when the actions are impletemented
    public class GenerateTestsActionHelper : IActionHelper
    {
        public async Task<bool> HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage, IActionHandlerServiceRepository repository)
        {
            return await Task.FromResult(true);
        }
    }
}
