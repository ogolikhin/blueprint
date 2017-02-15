using System;
using System.Web.Http;
using ServiceLibrary.Helpers;
using ServiceLibrary.Swagger;

namespace FileStore
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
#if DEBUG
            GlobalConfiguration.Configure(config => SwaggerConfig.Register(config, "~/bin/FileStore.XML", "FileStore",
                "FileStore is Web Service to persist and provide files attached to artifacts or any other kind of user files in Blueprint Web Application."));
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
    }
}
