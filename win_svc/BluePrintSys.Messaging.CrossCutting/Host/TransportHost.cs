using System;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Logging;
using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public class TransportHost : IMessageTransportHost
    {
        private readonly IConfigHelper _configHelper;
        private readonly INServiceBusServer _nServiceBusServer;

        public TransportHost(IConfigHelper configHelper, INServiceBusServer nServiceBusServer)
        {
            _configHelper = configHelper ?? new ConfigHelper();
            _nServiceBusServer = nServiceBusServer;
        }

        public async Task SendAsync(string tenantId, IWorkflowMessage message)
        {
            Log.Info("Sending message to server via RabbitMQ");
            await _nServiceBusServer.Send(tenantId, message);
        }

        public void Start(bool sendOnly, Func<bool> errorCallback = null)
        {
            Task.Run(() => _nServiceBusServer.Start(
                _configHelper.NServiceBusConnectionString,
                sendOnly)).ContinueWith(startTask =>
            {
                if (!string.IsNullOrEmpty(startTask.Result))
                {
                    Log.Error(startTask.Result);
                    errorCallback?.Invoke();
                }
            });
        }

        public void Stop()
        {
            _nServiceBusServer.Stop().Wait();
        }
    }
}
