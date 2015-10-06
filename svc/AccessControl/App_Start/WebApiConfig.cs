using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
		}

		public static string AdminStoreDatabase = ConfigurationManager.ConnectionStrings["AdminStoreDatabase"].ConnectionString;
		public static string SessionTimeoutInterval = ConfigurationManager.AppSettings["SessionTimeoutInterval"];
	}
}
