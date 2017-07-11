using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;

namespace NServiceBusSpike
{
    public class StateChangeMessage : IMessage
    {
        public Guid ArtifactId { get; set; }

        public override string ToString()
        {
            return ArtifactId.ToString();
        }
    }

    public class StateChangeMessageHandler : IHandleMessages<StateChangeMessage>
    {
        private static int _messagesHandled;

        public Task Handle(StateChangeMessage message, IMessageHandlerContext context)
        {
            Console.WriteLine(message);
            _messagesHandled++;

            var foreColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Handled {_messagesHandled} Statechange messages");

            Interlocked.Increment(ref Program.TotalMessagesHandled);
            Console.WriteLine($"Handled {Program.TotalMessagesHandled} total messages");

            Console.ForegroundColor = foreColor;

            var task = Task.Factory.StartNew(() => { });

            Program.NotificationMessageScheduler.Request(new NotificationMessage
            {
                NotificationId = Guid.NewGuid()
            });

            return task;
        }
    }
}
