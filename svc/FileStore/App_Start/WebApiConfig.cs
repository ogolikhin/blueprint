using System.Configuration;
using System.Web.Http;
using System.Web.Http.Hosting;

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

        public static string AccessControl = ConfigurationManager.AppSettings["AccessControl"];

        public static string ConfigControl = ConfigurationManager.AppSettings["ConfigControl"];

        internal static string LogSourceFiles = "FileStore.Files";
        internal static string LogSourceStatus = "FileStore.Status";
    }
}
