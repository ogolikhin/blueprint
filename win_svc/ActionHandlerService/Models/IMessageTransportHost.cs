using System;

namespace ActionHandlerService.Models
{
    public interface IMessageTransportHost
    {
        void Start(Func<bool> errorCallback = null);

        void Stop();
    }
}
