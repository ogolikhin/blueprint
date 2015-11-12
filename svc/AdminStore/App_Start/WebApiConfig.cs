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

        //TODO move the setting to database
        public static int MaximumInvalidLogonAttempts = ConfigValue("MaximumInvalidLogonAttempts", 5);

        //TODO move the setting to database
        public static bool VerifyCertificateChain = ConfigValue("VerifyCertificateChain", false);

        public static string ServiceLogName = ServiceLogSource + " Log";

        public static bool ConfigValue(string configValue, bool defaultValue)
        {
            return (ConfigurationManager.AppSettings[configValue] != null ? bool.Parse(ConfigurationManager.AppSettings[configValue].ToLower()) : defaultValue);
        }

        public static int ConfigValue(string configValue, int defaultValue)
        {
            return (ConfigurationManager.AppSettings[configValue] != null ? int.Parse(ConfigurationManager.AppSettings[configValue]) : defaultValue);
        }
    }
}
