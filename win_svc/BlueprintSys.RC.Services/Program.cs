using BlueprintSys.RC.Services.Helpers;
using Topshelf;

namespace BlueprintSys.RC.Services
{
    class Program
    {
        static void Main()
        {
            var serviceName = new ExtendedConfigHelper().ServiceName;
            HostFactory.Run(x =>
            {
                x.Service<ActionHandlerService>(s =>
                {
                    s.ConstructUsing(name => ActionHandlerService.Instance);
                    s.WhenStarted(tc => tc.Start(null));
                    s.WhenStopped(tc => tc.Stop(null));
                });
                x.RunAsLocalSystem();

                x.SetDescription("Processes Blueprint system messages.");
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
