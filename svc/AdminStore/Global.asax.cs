using System;
using System.Net;
using System.Web.Http;
using ServiceLibrary.Helpers;
using ServiceLibrary.Swagger;

namespace AdminStore
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
#if DEBUG
            GlobalConfiguration.Configure(config => SwaggerConfig.Register(config, "~/bin/AdminStore.XML", "AdminStore",
                "AdminStore is Web Service to facilitate application and project administration functionality, user authentication and authorization."));
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
