using System;
using System.Configuration;
using Actions = BluePrintSys.Messaging.Models.Actions;

namespace NServiceBusSpike
{
    class Program
    {
        public static volatile int TotalMessagesHandled = 0;
        public static StateChangeMessageScheduler StateChangeMessageScheduler { get; } = new StateChangeMessageScheduler();
        public static NotificationMessageScheduler NotificationMessageScheduler { get; } = new NotificationMessageScheduler();
        static void Main(string[] args)
        {
            bool createCounters = false;
            if (bool.TryParse(ConfigurationManager.AppSettings["CreateCounters"], out createCounters) && createCounters)
            {
                CounterCreator.Create();
            }

            ScheduleMessages();

            //StateChangeMessageScheduler.Request(new StateChangeMessage
            //{
            //    ArtifactId = Guid.NewGuid()
            //});
            //Console.ReadLine();
            Console.WriteLine("Ready to exit");
            Console.ReadLine();
            StopQueues();
        }

        static async void ScheduleMessages()
        {
            //for (int counter = 0; counter < 100; counter++)
            //{
            //    Console.WriteLine($"Scheduling new message: {counter + 1}");
                
            //    await MessageScheduler.Send(new Actions.NotificationMessage(1));
            //    Console.WriteLine($"Scheduled new message: {counter + 1}");
            //}

            for (int counter = 0; counter < 10; counter++)
            {
                await MessageScheduler.Send(new Actions.ArtifactsPublishedMessage
                {
                    TenantId = 1,
                    PublishedArtifacts = new []
                    {
                        new Actions.PublishedArtifact
                        {
                            ArtifactId = 1,
                            ArtifactTypeId = 2,
                            PredefinedArtifactTypeId = 3,
                            RevisionId = 4,
                            UserId = 5
                        }
                    }
                });
            }
        }

        private static async void StopQueues()
        {
            var scEndpoint = await StateChangeMessageScheduler.EndPoint.Value;
            if (scEndpoint != null)
            {
                await scEndpoint.Stop();
            }
            Console.WriteLine("Stopped State change queue");
            var nmEndpoint = await NotificationMessageScheduler.EndPoint.Value;
            if (nmEndpoint != null)
            {
                await nmEndpoint.Stop();
            }
            Console.WriteLine("Stopped notification queue");

            Console.WriteLine("Stopped all queues");
        }

        

    }
}
