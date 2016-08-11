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
            GlobalConfiguration.Configure(config => SwaggerConfig.Register(config, "AccessControl", "~/bin/AccessControl.XML"));
#endif
        }
    }
}
