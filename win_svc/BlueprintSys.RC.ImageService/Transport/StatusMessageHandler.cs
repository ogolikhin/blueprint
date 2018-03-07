using NServiceBus;
using System.Threading.Tasks;
using BluePrintSys.Messaging.Models.ProcessImageGeneration;
using BluePrintSys.Messaging.CrossCutting.Logging;

namespace BlueprintSys.RC.ImageService.Transport
{
    // ImageGen Status message handler
    public class StatusMessageHandler : IHandleMessages<StatusCheckMessage>
    {
        public async Task Handle(StatusCheckMessage message, IMessageHandlerContext context)
        {
            Log.Debug("ImageGen status message processed");
            await Task.FromResult(true);
        }
    }
}
