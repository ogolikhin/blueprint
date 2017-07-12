using System;
using System.Threading.Tasks;
using ActionHandlerService.Helpers;
using BluePrintSys.Messaging.CrossCutting.Logging;

namespace ActionHandlerService.Models
{
    public class RabbitMQTransportHost : IMessageTransportHost
    {
        private static readonly string NServiceBusConnectionString = ConfigHelper.NServiceBusConnectionString;
        private readonly NServiceBusServer _nServiceBusServer = new NServiceBusServer();
        public void Start(Func<bool> errorCallback = null)
        {
            Task.Run(() => _nServiceBusServer.Start(NServiceBusConnectionString))
                    .ContinueWith(startTask =>
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
