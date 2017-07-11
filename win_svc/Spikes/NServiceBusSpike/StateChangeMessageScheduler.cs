using System;
using System.Configuration;
using NServiceBus;

namespace NServiceBusSpike
{
    public class StateChangeMessageScheduler
    {
        private const string NServiceBusConnectionString = "host=titan.blueprintsys.net;virtualhost=workflowtest;username=admin;password=$admin2011";
        public static readonly AsyncLazy<IEndpointInstance> EndPoint = 
            new AsyncLazy<IEndpointInstance>(async () => await EndpointCreator.CreateEndPoint(NServiceBusConnectionString));
        private const string Handler = "Messaging.Blueprint.WorkflowServer";

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
                options.SetDestination(Handler);

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
