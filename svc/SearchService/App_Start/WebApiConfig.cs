﻿using System.Configuration;
using System.Web.Http;

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

        public static string AccessControl = ConfigurationManager.AppSettings["AccessControl"];

        public static string ConfigControl = ConfigurationManager.AppSettings["ConfigControl"];

        public static string StatusCheckPreauthorizedKey = ConfigurationManager.AppSettings["StatusCheckPreauthorizedKey"];

        internal static string LogSourceStatus = "SearchService.Status";
    }
}
