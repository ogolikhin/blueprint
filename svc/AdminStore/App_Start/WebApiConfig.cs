using System.Configuration;
using System.Reflection;
using System.Web.Http;

namespace AdminStore
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
        }

        public static string AdminStorage = ConfigurationManager.ConnectionStrings["AdminStorage"].ConnectionString;

        public static string RaptorMain = ConfigurationManager.ConnectionStrings["RaptorMain"].ConnectionString;

        public static string AccessControl = ConfigurationManager.AppSettings["AccessControl"];

        public static string ConfigControl = ConfigurationManager.AppSettings["ConfigControl"];

        public static string ServiceLogSource = typeof(WebApiConfig).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0].ToString();

        public static string MaximumInvalidLogonAttempts = ConfigurationManager.AppSettings["MaximumInvalidLogonAttempts"];

        public static string ServiceLogName = ServiceLogSource + " Log";
    }
}
