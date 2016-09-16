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

        public static string AccessControl = ConfigurationManager.AppSettings["AccessControl"];

        public static string ConfigControl = ConfigurationManager.AppSettings["ConfigControl"];

        public static string StatusCheckPreauthorizedKey = ConfigurationManager.AppSettings["StatusCheckPreauthorizedKey"];

        public static int PageSize = ConfigurationManager.AppSettings["PageSize"].ToInt32(10);
    }
}
