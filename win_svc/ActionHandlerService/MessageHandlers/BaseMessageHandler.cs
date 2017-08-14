﻿using System;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using ActionHandlerService.Models;
using ActionHandlerService.Models.Exceptions;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;

namespace ActionHandlerService.MessageHandlers
{
    public abstract class BaseMessageHandler<T> : IHandleMessages<T> where T : ActionMessage
    {
        private IActionHelper ActionHelper { get; }
        private ITenantInfoRetriever TenantInfoRetriever { get; }
        private IConfigHelper ConfigHelper { get; }

        protected BaseMessageHandler(IActionHelper actionHelper, ITenantInfoRetriever tenantInfoRetriever, IConfigHelper configHelper)
        {
            ActionHelper = actionHelper;
            TenantInfoRetriever = tenantInfoRetriever;
            ConfigHelper = configHelper;
        }

        public async Task Handle(T message, IMessageHandlerContext context)
        {
            try
            {
                Log.Info($"Received Action Message {message.ActionType.ToString()}");
                if ((ConfigHelper.SupportedActionTypes & message.ActionType) == message.ActionType)
                {
                    var tentantId = GetMessageHeaderValue(ActionMessageHeaders.TenantId, context);
                    var tenants = await TenantInfoRetriever.GetTenants();
                    TenantInformation tenant;
                    if (!tenants.TryGetValue(tentantId, out tenant))
                    {
                        Log.Error($"Tenant Info not found for Tenant ID {tentantId}. Message is not processed.");
                        return;
                    }
                    await ProcessAction(tenant, message, context);
                }
                else
                {
                    throw new UnsupportedActionTypeException($"Unsupported Action Type: {message.ActionType.ToString()}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to handle {message.ActionType.ToString()} message due to an exception: {ex.Message}", ex);
                throw;
            }
        }

        private string GetMessageHeaderValue(string header, IMessageHandlerContext context)
        {
            string headerValue;
            if (!context.MessageHeaders.TryGetValue(header, out headerValue))
            {
                throw new MessageHeaderValueNotFoundException($"Message Header Value Not Found: {header}");
            }
            return headerValue;
        }

        protected virtual async Task<bool> ProcessAction(TenantInformation tenant, T message, IMessageHandlerContext context)
        {
            Log.Info($"{message.ActionType.ToString()} action handling started for tenant {tenant.Id}");
            IActionHandlerServiceRepository serviceRepository;
            if (message is NotificationMessage)
            {
                serviceRepository = new NotificationActionHandlerServiceRepository(tenant.ConnectionString);
            }
            else
            {
                serviceRepository = new ActionHandlerServiceRepository(tenant.ConnectionString);
            }
            
            return await ActionHelper.HandleAction(tenant, message, serviceRepository);
        }
    }
}
