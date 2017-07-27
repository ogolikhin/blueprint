using System;
using System.Threading.Tasks;
using NServiceBus;

namespace PocService
{
    /// <summary>
    /// Proof Of Concept for a service with a message handler that can spawn additional messages
    /// </summary>
    public class Program
    {
        public static IEndpointInstance EndpointInstance;

        public static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        private static async Task AsyncMain()
        {
            const string endpointName = "PocService";
            Console.Title = endpointName;
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.UseTransport<MsmqTransport>();
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.AddHeaderToAllOutgoingMessages(PocHeaders.TenantId, "1");
            EndpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            await RunLoop().ConfigureAwait(false);
            await EndpointInstance.Stop().ConfigureAwait(false);
        }

        private static async Task RunLoop()
        {
            while (true)
            {
                Console.WriteLine("Press 'm' to send a message, or any other key to quit.");
                if (Console.ReadKey().Key == ConsoleKey.M)
                {
                    Console.WriteLine();
                    Console.WriteLine("Sending message.");
                    await EndpointInstance.SendLocal(new PocMessage {Id = 0}).ConfigureAwait(false);
                }
                else
                {
                    return;
                }
            }
        }
    }

    [Express]
    public class PocMessage : IMessage
    {
        public int Id { get; set; }
    }

    public class PocHeaders
    {
        public const string TenantId = "TenantId";
        public const string MessageId = Headers.MessageId;
        public const string TimeSent = Headers.TimeSent;
    }

    public class PocMessageHandler : IHandleMessages<PocMessage>
    {
        public Task Handle(PocMessage message, IMessageHandlerContext context)
        {
            string tenantId;
            if (!context.MessageHeaders.TryGetValue(PocHeaders.TenantId, out tenantId)) throw new Exception("Message Header Not Found");

            Program.EndpointInstance.SendLocal(new PocSpawnMessage {Id = 1}).ConfigureAwait(false);
            Program.EndpointInstance.SendLocal(new PocAnotherSpawnMessage {Id = 2}).ConfigureAwait(false);
            Program.EndpointInstance.SendLocal(new PocAnotherSpawnMessage {Id = 3}).ConfigureAwait(false);

            return Task.CompletedTask;
        }
    }

    [Express]
    public class PocSpawnMessage : IMessage
    {
        public int Id { get; set; }
    }

    public class PocSpawnMessageHandler : IHandleMessages<PocSpawnMessage>
    {
        public Task Handle(PocSpawnMessage message, IMessageHandlerContext context)
        {
            return Task.CompletedTask;
        }
    }

    [Express]
    public class PocAnotherSpawnMessage : IMessage
    {
        public int Id { get; set; }
    }

    public class PocAnotherSpawnMessageHandler : IHandleMessages<PocAnotherSpawnMessage>
    {
        public Task Handle(PocAnotherSpawnMessage message, IMessageHandlerContext context)
        {
            return Task.CompletedTask;
        }
    }
}
