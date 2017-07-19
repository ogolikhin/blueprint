using System;
using System.Threading;
using System.Threading.Tasks;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;

namespace NServiceBusSpike
{
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

            Program.StateChangeMessageScheduler.Request(new StateChangeMessage
            {
               TenantId = new Random(50).Next(1,100)
            });

            return task;
        }
    }
}
