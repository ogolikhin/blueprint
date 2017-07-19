using System;
using System.Threading.Tasks;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;

namespace NServiceBusSpike
{
    public class ArtifactsPublishedMessageHandler : IHandleMessages<ArtifactsPublishedMessage>
    {
        public Task Handle(ArtifactsPublishedMessage message, IMessageHandlerContext context)
        {
            Console.WriteLine("Handling artifacts published message");
            return Task.Factory.StartNew(() => { });
        }
    }
}
