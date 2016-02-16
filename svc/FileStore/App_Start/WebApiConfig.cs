using System.Web.Http;
using System.Web.Http.Hosting;
using ServiceLibrary.Helpers;

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

            // Init shared HttpClients
            ConfigControlHttpClientLocator.InitDefaultInstance();
        }

        internal static string LogSourceFiles = "FileStore.Files";
        internal static string LogSourceStatus = "FileStore.Status";
    }
}
