using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Topshelf;

namespace ActionHandlerService
{
    public class ActionHandlerService : ServiceControl
    {
        private HttpSelfHostServer _server;
        private readonly HttpSelfHostConfiguration _config;
        public readonly Uri ServiceAddress = new Uri(@"http://localhost:5557");

        private static readonly string NServiceBusConnectionString = ConfigHelper.NServiceBusConnectionString;
        private readonly NServiceBusServer _nServiceBusServer = new NServiceBusServer();

        public IActionHandlerHelper ActionHandlerHelper = new ActionHandlerHelper();

        private static ActionHandlerService _instance;
        public static ActionHandlerService Instance => _instance ?? (_instance = new ActionHandlerService());

        private ActionHandlerService()
        {
            _config = new HttpSelfHostConfiguration(ServiceAddress);
            _config.Routes.MapHttpRoute("Api", "api/{controller}", new {url = RouteParameter.Optional});
        }

        public bool Start(HostControl hostControl)
        {
            _server = new HttpSelfHostServer(_config);
            _server.OpenAsync().Wait();
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
            _server.CloseAsync().Wait();
            _server.Dispose();
            return true;
        }
    }
}
