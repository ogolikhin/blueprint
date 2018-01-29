using NServiceBus;
using System.Threading.Tasks;
using BluePrintSys.Messaging.Models.Actions;

namespace BlueprintSys.RC.Services.MessageHandlers
{
    // Workflow Status message handler
    public class StatusMessageHandler : IHandleMessages<StatusCheckMessage>
    {
        public async Task Handle(StatusCheckMessage message, IMessageHandlerContext context)
        {
            await Task.FromResult(true);
        }
    }
}
