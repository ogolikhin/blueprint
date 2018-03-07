using BlueprintSys.RC.Services.Helpers;
using Topshelf;

namespace BlueprintSys.RC.Services
{
    class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        static void Main()
        {
            var serviceName = new ExtendedConfigHelper().ServiceName;
            HostFactory.Run(x =>
            {
                x.Service<ActionHandlerService>(s =>
                {
                    s.ConstructUsing(name => ActionHandlerService.Instance);
                    s.WhenStarted((tc, hostControl) => tc.Start(hostControl));
                    s.WhenStopped((tc, hostControl) => tc.Stop(hostControl));
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
