using System;
using System.Configuration;
using System.Reflection;
using System.Web.Http;

namespace AccessControl
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            Controllers.SessionsController.Load();
        }

        public static string AdminStoreDatabase = ConfigurationManager.ConnectionStrings["AdminStoreDatabase"].ConnectionString;

        public static int SessionTimeoutInterval = Int32.Parse(ConfigurationManager.AppSettings["SessionTimeoutInterval"]);

        public static string ServiceLogSource =
            typeof (WebApiConfig).Assembly.GetCustomAttributes(typeof (AssemblyTitleAttribute), false)[0].ToString();

        public static string ServiceLogName = ServiceLogSource + " Log";
    }
}
