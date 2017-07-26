﻿using ActionHandlerService.Helpers;
using BluePrintSys.Messaging.Models.Actions;

namespace ActionHandlerService.MessageHandlers.Notifications
{
    public class NotificationMessageHandler : BaseMessageHandler<NotificationMessage>
    {
        public NotificationMessageHandler() : this(new NotificationsActionHelper())
        {
        }

        public NotificationMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever = null, IConfigHelper configHelper = null) : base(actionHelper, tenantInfoRetriever, configHelper)
        {
        }
    }
}
