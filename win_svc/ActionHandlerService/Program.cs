using ActionHandlerService.Helpers;
using Topshelf;

namespace ActionHandlerService
{
    class Program
    {
        static void Main()
        {
            var serviceName = new ConfigHelper().ServiceName;
            HostFactory.Run(x =>
            {
                x.Service<ActionHandlerService>(s =>
                {
                    s.ConstructUsing(name => ActionHandlerService.Instance);
                    s.WhenStarted(tc => tc.Start(null));
                    s.WhenStopped(tc => tc.Stop(null));
                });
                x.RunAsLocalSystem();

                x.SetDescription("Action Handler");
                x.SetDisplayName($"{serviceName} service");
                x.SetServiceName(serviceName);

                x.StartAutomatically();
                x.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1);
                });
            });
        }
    }
}
