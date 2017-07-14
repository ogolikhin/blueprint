using System;
using System.Configuration;
using System.Threading.Tasks;
using BluePrintSys.Messaging.Models.Actions;
using NServiceBus;

namespace NServiceBusSpike
{
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) :
            base(() => Task.Factory.StartNew(valueFactory))
        { }
        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap())
        { }
    }

    public class NotificationMessageScheduler
    {
        private static readonly string NServiceBusConnectionString = ConfigurationManager.AppSettings["NServiceBus.ConnectionString"];
        private static readonly string QueueName = ConfigurationManager.AppSettings["QueueName"];
        public static readonly AsyncLazy<IEndpointInstance> EndPoint =
            new AsyncLazy<IEndpointInstance>(async () => await EndpointCreator.CreateEndPoint(NServiceBusConnectionString, QueueName));
        private static int _messageScheduled;

        public async void Request(NotificationMessage requestData)
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
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Scheduled {_messageScheduled} Notification message");
                Console.ForegroundColor = foreColor;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }

    
}


