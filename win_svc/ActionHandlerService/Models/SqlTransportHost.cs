using System;

namespace ActionHandlerService.Models
{
    public class SqlTransportHost : IMessageTransportHost
    {
        public void Start(Func<bool> errorCallback = null)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
