using System.Web.Http;

namespace ArtifactStore
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
#if DEBUG
            GlobalConfiguration.Configure(SwaggerConfig.Register);
#endif
        }
    }
}
