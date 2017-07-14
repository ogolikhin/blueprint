using System;
using System.Configuration;
using System.Threading.Tasks;
using NServiceBus;

namespace NServiceBusSpike
{
    public class MessageScheduler
    {
        private static readonly string NServiceBusConnectionString = ConfigurationManager.AppSettings["NServiceBus.ConnectionString"];
        private static readonly string QueueName = ConfigurationManager.AppSettings["QueueName"];
        public static readonly AsyncLazy<IEndpointInstance> EndPoint =
            new AsyncLazy<IEndpointInstance>(async () => await EndpointCreator.CreateEndPoint(NServiceBusConnectionString, QueueName));

        private static int _messageScheduled;

        public static async Task Send<T>(T requestData)
        {
            try
            {
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

//namespace BluePrintSys.Messaging.Models.Actions
//{
//    [Flags]
//    public enum MessageActionType
//    {
//        None = 0,
//        Property = 1,
//        Notification = 2,
//        GenerateDescendants = 4,
//        GenerateTests = 8,
//        GenerateUserStories = 16,
//        All = Property | Notification | GenerateDescendants | GenerateTests | GenerateUserStories
//    }

//    [Express]
//    public abstract class ActionMessage : IMessage
//    {
//        protected ActionMessage(MessageActionType actionType, int tenantId, int workflowId)
//        {
//            ActionType = actionType;
//            TenantId = tenantId;
//            WorkflowId = workflowId;
//        }

//        public MessageActionType ActionType { get; set; }
//        public int TenantId { get; set; }
//        public int WorkflowId { get; set; }
//    }

//    [Express]
//    public class NotificationMessage : ActionMessage
//    {
//        public NotificationMessage(int tenantId, int workflowId) : base(MessageActionType.Notification, tenantId, workflowId)
//        {
//        }
//    }
//}
