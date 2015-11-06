using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

			var x = Controllers.SessionsController.Trigger;
		}

		public static string AdminStoreDatabase = ConfigurationManager.ConnectionStrings["AdminStoreDatabase"].ConnectionString;

		public static int SessionTimeoutInterval = Int32.Parse(ConfigurationManager.AppSettings["SessionTimeoutInterval"]);

		public static string ServiceLogSource =
			typeof (WebApiConfig).Assembly.GetCustomAttributes(typeof (AssemblyTitleAttribute), false)[0].ToString();

		public static string ServiceLogName = ServiceLogSource + " Log";
	}
}
