using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;

namespace NServiceBusSpike
{
    public class NotificationMessage : IMessage
    {
        public Guid NotificationId { get; set; }

        public override string ToString()
        {
            return NotificationId.ToString();
        }
    }

    public class NotificationMessageHandler : IHandleMessages<NotificationMessage>
    {
        private static int _messagesHandled;
        public Task Handle(NotificationMessage message, IMessageHandlerContext context)
        {
            Console.WriteLine(message);
            _messagesHandled++;

            var foreColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Handled {_messagesHandled} notification messages");

            Interlocked.Increment(ref Program.TotalMessagesHandled);
            Console.WriteLine($"Handled {Program.TotalMessagesHandled} total messages");

            Console.ForegroundColor = foreColor;

            var task = Task.Factory.StartNew(() => { });

            Program.StateChangeMessageScheduler.Request(new StateChangeMessage()
            {
                ArtifactId = Guid.NewGuid()
            });

            return task;
        }
    }
}
