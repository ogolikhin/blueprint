using System.Configuration;
using System.Web.Http;
using ServiceLibrary.Helpers;

namespace SearchService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

        }

        public static string BlueprintConnectionString = ConfigurationManager.ConnectionStrings["Blueprint"].ConnectionString;

        public static string AccessControl = AppSettingsHelper.TryGetConfigurationValue<string>("AccessControl");

        public static string ConfigControl = AppSettingsHelper.TryGetConfigurationValue<string>("ConfigControl");

        public static string StatusCheckPreauthorizedKey = AppSettingsHelper.TryGetConfigurationValue<string>("StatusCheckPreauthorizedKey");

        /// <summary>
        /// Search Sql Timeout in seconds. Default is 120 seconds. 
        /// </summary>
        public static int SearchTimeout = AppSettingsHelper.TryGetConfigurationValue("SearchTimeout", 120);

        
    }
}
