using System.Web;
using System.Web.Http;
using ServiceLibrary.Swagger;

namespace AccessControl
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
#if DEBUG
            GlobalConfiguration.Configure(config => SwaggerConfig.Register(config, "~/bin/AccessControl.XML", "AccessControl",
                "AccessControl is Auxiliary Web Service to trace user sessions and resolve authorization queries within Blueprint services. AccessControl is the internal part of enterprise solution infrastructure and service endpoint is not visible to client web application."));
#endif
        }
    }
}
