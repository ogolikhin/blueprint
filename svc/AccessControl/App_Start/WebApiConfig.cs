using System.Configuration;
using System.Web.Http;
using AccessControl.Helpers;

namespace AccessControl
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

        public static int SessionTimeoutInterval = int.Parse(ConfigurationManager.AppSettings["SessionTimeoutInterval"]);

        public static int LicenseHoldTime = LicenceHelper.GetLicenseHoldTime(
            ConfigurationManager.AppSettings["LHTSetting"], 1440);

        internal static string LogSource_Sessions= "AccessControl.Sessions";

        internal static string LogSource_Licenses = "AccessControl.Licenses";
    }
}
