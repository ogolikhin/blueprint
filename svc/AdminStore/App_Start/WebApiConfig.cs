using System.Configuration;
using System.Reflection;
using System.Web.Http;
using ServiceLibrary.Helpers;

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
            return (ConfigurationManager.AppSettings[configValue] != null ? bool.Parse(ConfigurationManager.AppSettings[configValue]) : defaultValue);
        }

        public static int ConfigValue(string configValue, int defaultValue)
        {
            return (ConfigurationManager.AppSettings[configValue] != null ? I18NHelper.Int32ParseInvariant(ConfigurationManager.AppSettings[configValue]) : defaultValue);
        }

        internal static string LogSource_Config = "AdminStore.Config";
        internal static string LogSource_Licenses = "AdminStore.Licenses";
        internal static string LogSource_Sessions = "AdminStore.Sessions";
        internal static string LogSource_Users = "AdminStore.Users";
    }
}
