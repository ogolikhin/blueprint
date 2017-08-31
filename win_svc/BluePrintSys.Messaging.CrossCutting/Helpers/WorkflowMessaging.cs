using System;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Host;
using BluePrintSys.Messaging.CrossCutting.Logging;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.CrossCutting.Helpers
{
    public delegate Task SendMessageAsyncDelegate(string tenantId, IWorkflowMessage message);

    public class WorkflowMessaging
    {
        private readonly IMessageTransportHost _messageTransportHost;
        private static readonly Lazy<WorkflowMessaging> _instance = new Lazy<WorkflowMessaging>(() => new WorkflowMessaging());
        public static WorkflowMessaging Instance => _instance.Value;

        public static SendMessageAsyncDelegate SendMessageAsyncDelegate =
            async (tenantId, messageEvent) =>
            {
                await Instance.SendMessageAsync(tenantId, messageEvent);
            };

        private WorkflowMessaging()
        {
            Log.Debug("Workflow Messaging: Started opening the endpoint....");
            _messageTransportHost = new TransportHost(new ConfigHelper(), GenericServiceBusServer.Instance);

            Task.Factory.StartNew(() => _messageTransportHost.Start(true)).Wait();

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
