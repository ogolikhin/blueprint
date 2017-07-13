using ActionHandlerService.Helpers;
using Topshelf;

namespace ActionHandlerService
{
    class Program
    {
        static void Main()
        {
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
                x.SetDisplayName($"{ConfigHelper.ServiceName} service");
                x.SetServiceName(ConfigHelper.ServiceName);

                x.StartAutomatically();
                x.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1);
                });
            });
        }
    }
}
