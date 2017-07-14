﻿using ActionHandlerService.Models;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.GenerateTests
{
    //We should be creating specific action handlers for different  message handlers. 
    //These should be implemented when the actions are impletemented
    public class GenerateTestsActionHelper : IActionHelper
    {
        public bool HandleAction(TenantInformation tenantInformation, ActionMessage actionMessage)
        {
            return true;
        }
    }
}
