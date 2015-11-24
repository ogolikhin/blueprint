using System.Web.Http;
using System.Web.Http.Hosting;
using FileStore.Controllers;

namespace FileStore
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHostBufferPolicySelector), new FileUploadBufferPolicySelector());

            // Web API routes
            config.MapHttpAttributeRoutes();
        }
    }
}
