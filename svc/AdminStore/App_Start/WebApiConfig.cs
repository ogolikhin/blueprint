using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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

		public static string AdminStoreDatabase = ConfigurationManager.ConnectionStrings["AdminStoreDatabase"].ConnectionString;

		public static string RaptorMainDatabase = ConfigurationManager.ConnectionStrings["RaptorMainDatabase"].ConnectionString;

		public static string AccessControlSvc = ConfigurationManager.AppSettings["AccessControlSvc"];

		public static string ServiceLogSource =
			typeof(WebApiConfig).Assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0].ToString();

		public static string ServiceLogName = ServiceLogSource + " Log";
	}
}
