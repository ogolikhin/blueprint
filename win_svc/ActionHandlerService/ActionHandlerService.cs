using System.Threading.Tasks;
using Topshelf;

namespace ActionHandlerService
{
    public class ActionHandlerService : ServiceControl
    {

        private static readonly string NServiceBusConnectionString = ConfigHelper.NServiceBusConnectionString;
        private readonly NServiceBusServer _nServiceBusServer = new NServiceBusServer();

        private static ActionHandlerService _instance;
        public static ActionHandlerService Instance => _instance ?? (_instance = new ActionHandlerService());

        private ActionHandlerService() { }

        public bool Start(HostControl hostControl)
        {
            Task.Run(() => _nServiceBusServer.Start(NServiceBusConnectionString))
                .ContinueWith(startTask =>
                {
                    if (!string.IsNullOrEmpty(startTask.Result))
                    {
                        Stop(null);
                    }
                });
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _nServiceBusServer.Stop().Wait();
            return true;
        }
    }
}
