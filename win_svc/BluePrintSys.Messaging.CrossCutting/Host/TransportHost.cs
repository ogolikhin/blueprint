using System;
using System.Threading.Tasks;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.CrossCutting.Logging;
using ServiceLibrary.Models.Workflow;
using BluePrintSys.Messaging.CrossCutting.Models;

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
            Log.Info("Sending message to server.");
            await _nServiceBusServer.Send(tenantId, message);
        }

        public async Task GetStatusAsync(int timeout)
        {
            Log.Info("Sending Status message to server.");

            var task = _nServiceBusServer.GetStatus(new StatusCheckMessage());

            if (await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(timeout))) == task)
            {
                return;
            }
            else
            {
                string message = "Check Status Timeout.";
                Log.Error(message);
                throw new Exception(message);
            }
        }

        public async Task Start(bool sendOnly, Func<bool> errorCallback = null)
        {
            var result = await _nServiceBusServer.Start(_configHelper.NServiceBusConnectionString, sendOnly);
            if (!string.IsNullOrEmpty(result))
            {
                Log.Error(result);
                errorCallback?.Invoke();
            }
        }

        public void Stop()
        {
            _nServiceBusServer.Stop().Wait();
        }
    }
}
