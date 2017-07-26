using System;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using BluePrintSys.Messaging.CrossCutting.Logging;

namespace ActionHandlerService.Models
{
    public class RabbitMqTransportHost : IMessageTransportHost
    {
        private readonly IConfigHelper _configHelper;
        private readonly INServiceBusServer _nServiceBusServer;

        public RabbitMqTransportHost(IConfigHelper configHelper = null, INServiceBusServer nServiceBusServer = null)
        {
            _configHelper = configHelper ?? new ConfigHelper();
            _nServiceBusServer = nServiceBusServer ?? NServiceBusServer.Instance;
        }

        public void Start(Func<bool> errorCallback = null)
        {
            Task.Run(() => _nServiceBusServer.Start(_configHelper.NServiceBusConnectionString)).ContinueWith(startTask =>
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
