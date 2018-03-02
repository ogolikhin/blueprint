using BlueprintSys.RC.ImageService.Helpers;
using BlueprintSys.RC.ImageService.ImageGen;
using CefSharp;
using System;
using Topshelf;

namespace BlueprintSys.RC.ImageService
{
    class Program
    {
        /* 1. Make sure `VC++ 2013 Redist` is installed (either `x86` or `x64` depending on your application).
         * 2. [Resolved] CefSharp - Shutdown must be called on the same thread that Initialize was called. Topshelf approach below causes an error when the service is stopped.
         * 3. There is no event to hook up when the browser resizing is completed. The delay is used in this spike.
         */
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<ImageGenService>(s =>
                {
                    s.ConstructUsing(name => ImageGenService.Instance);
                    s.WhenStarted((tc, hostControl) => tc.Start(hostControl));
                    s.WhenStopped((tc, hostControl) => tc.Stop(hostControl));
                });
                x.RunAsLocalSystem();

                x.SetDescription("Generate Process images.");
                x.SetDisplayName($"{ServiceHelper.ServiceName} service");
                x.SetServiceName(ServiceHelper.ServiceName);

                x.StartAutomatically();
                x.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1);
                });
            });

            // See #2 above.
            if (Environment.UserInteractive)
            {
                Cef.Shutdown();
            }
        }
    }
}

