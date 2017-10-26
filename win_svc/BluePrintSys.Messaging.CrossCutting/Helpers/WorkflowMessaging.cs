using System;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.CrossCutting.Logging;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.CrossCutting.Helpers
{
    public delegate Task SendMessageAsyncDelegate(string tenantId, IWorkflowMessage message);

    public interface IWorkflowMessagingProcessor
    {
        Task SendMessageAsync(string tenantId, IWorkflowMessage message);
    }

    public class WorkflowMessagingProcessor : IWorkflowMessagingProcessor
    {
        private readonly IMessageTransportHost _messageTransportHost;
        private static readonly Lazy<WorkflowMessagingProcessor> _instance = new Lazy<WorkflowMessagingProcessor>(() => new WorkflowMessagingProcessor());
        public static WorkflowMessagingProcessor Instance => _instance.Value;

        public static SendMessageAsyncDelegate SendMessageAsyncDelegate =
            async (tenantId, messageEvent) =>
            {
                await Instance.SendMessageAsync(tenantId, messageEvent);
            };

        private WorkflowMessagingProcessor()
        {
            Log.Debug("Workflow Messaging: Started opening the endpoint....");
            _messageTransportHost = new TransportHost(new ConfigHelper(), GenericServiceBusServer.Instance);

            Task.Factory.StartNew(async () => await _messageTransportHost.Start(true)).Unwrap().Wait();

            Log.Debug("Workflow Messaging: Finished opening the endpoint.");
        }

        public async Task SendMessageAsync(string tenantId, IWorkflowMessage message)
        {
            await _messageTransportHost.SendAsync(tenantId, message);
        }

        public static void Shutdown()
        {
            if (!_instance.IsValueCreated)
            {
                return;
            }

            _instance?.Value.Stop();
            Log.Debug("Workflow Messaging: the endpoint stopped.");
        }
        private void Stop()
        {
            var task = Task.Factory.StartNew(() => _messageTransportHost.Stop());
            task.Wait();
        }
    }
}
