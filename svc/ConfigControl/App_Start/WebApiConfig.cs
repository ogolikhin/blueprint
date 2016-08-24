using System.Configuration;
using System.Web.Http;
using ServiceLibrary.Attributes;

namespace ConfigControl
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Filters.Add(new UnhandledExceptionFilterAttribute());
        }

        public static string AdminStorage = ConfigurationManager.ConnectionStrings["AdminStorage"].ConnectionString;

        public static string AccessControl = ConfigurationManager.AppSettings["AccessControl"];

        internal static string LogSourceStatus = "ConfigControl.Status";
        internal static string LogRecordStatus = "ConfigControl.GetLogs";
    }
}
