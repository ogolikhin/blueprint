using System;
using System.Configuration;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;

namespace NServiceBusSpike
{
    public class StateChangeMessageScheduler
    {
        private static readonly string NServiceBusConnectionString = ConfigurationManager.AppSettings["NServiceBus.ConnectionString"];
        private static readonly string QueueName = ConfigurationManager.AppSettings["QueueName"];
        public static readonly AsyncLazy<IEndpointInstance> EndPoint = 
            new AsyncLazy<IEndpointInstance>(async () => await EndpointCreator.CreateEndPoint(NServiceBusConnectionString, QueueName));

        private static int _messageScheduled;

        public async void Request(StateChangeMessage requestData)
        {
            try
            {
                bool enabled;
                if (!bool.TryParse(ConfigurationManager.AppSettings["ServerEnabled"], out enabled) || !enabled)
                {
                    return;
                }

                var endPoint = await EndPoint.Value;

                var options = new SendOptions();
                options.SetDestination(QueueName);

                await endPoint.Send(requestData, options);
                _messageScheduled++;

                var foreColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"Scheduled {_messageScheduled} StateChange message");
                Console.ForegroundColor = foreColor;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
