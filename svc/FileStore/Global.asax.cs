using System.Web.Http;
using ServiceLibrary.Swagger;

namespace FileStore
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
#if DEBUG
            GlobalConfiguration.Configure(config => SwaggerConfig.Register(config, "FileStore", "~/bin/FileStore.XML"));
#endif
        }
    }
}
