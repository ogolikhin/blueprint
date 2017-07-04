using System;
using System.Threading.Tasks;
using BluePrintSys.ActionMessaging.Models;
using NServiceBus;

namespace ActionHandlerService.MessageHandlers
{
    public class GenerateDescendantsMessageHandler : IHandleMessages<GenerateDescendantsMessage>
    {
        public Task Handle(GenerateDescendantsMessage message, IMessageHandlerContext context)
        {
            try
            {
                if (ConfigHelper.AllowedActionTypes.Contains(message.ActionType))
                {
                    var tenantId = message.TenantId;
                    var tenants = TenantInfoRetriever.GetTenants();
                    TenantInfo tenant;
                    if (!tenants.TryGetValue(tenantId, out tenant))
                    {
                        throw new Exception($"Tentant Info not found for Tenant ID {tenantId}");
                    }
                    ActionHandlerService.Instance.ActionHandlerHelper.HandleAction(tenant);
                }
                else
                {
                    throw new Exception($"Unsupported Action Type: {message.ActionType.ToString()}");
                }
            }
            catch (Exception)
            {
                //todo log exception
                throw;
            }
            return Task.CompletedTask;
        }
    }
}
