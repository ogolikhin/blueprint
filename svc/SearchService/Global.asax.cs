using System.Web.Http;
using ServiceLibrary.Swagger;

namespace SearchService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
#if DEBUG
            GlobalConfiguration.Configure(config => SwaggerConfig.Register(config, "~/bin/SearchService.XML", "SearchService",
                "SearchService is Web Service to perform searchs in Blueprint Web Application."));
#endif
        }
    }
}
