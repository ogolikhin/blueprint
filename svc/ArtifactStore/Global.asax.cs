using System;
using System.Web.Http;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using ServiceLibrary.Helpers;
using ServiceLibrary.Swagger;

namespace ArtifactStore
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
#if DEBUG
            GlobalConfiguration.Configure(config => SwaggerConfig.Register(config, "~/bin/ArtifactStore.XML", "ArtifactStore"));
#endif
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            if (ex == null)
            {
                return;
            }
            var jsonFormatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            var xmlFormatter = GlobalConfiguration.Configuration.Formatters.XmlFormatter;
            ServerHelper.UpdateResponseWithError(Request, Response, jsonFormatter, xmlFormatter, ex.Message);
            Server.ClearError();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            WorkflowMessaging.Shutdown();
        }
    }
}
