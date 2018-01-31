using NServiceBus;
using System.Threading.Tasks;
using BluePrintSys.Messaging.Models.ProcessImageGeneration;

namespace BlueprintSys.RC.ImageService.Transport
{
    // ImageGen Status message handler
    public class StatusMessageHandler : IHandleMessages<StatusCheckMessage>
    {
        public async Task Handle(StatusCheckMessage message, IMessageHandlerContext context)
        {
            await Task.FromResult(true);
        }
    }
}
