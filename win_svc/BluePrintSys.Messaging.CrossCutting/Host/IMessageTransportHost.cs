using System;
using System.Threading.Tasks;
using NServiceBus;

namespace BluePrintSys.Messaging.CrossCutting.Host
{
    public interface IMessageTransportHost
    {
        void Start(bool sendOnly, Func<bool> errorCallback = null);

        void Stop();

        Task SendAsync(string tenantId, IMessage message);
    }
}
