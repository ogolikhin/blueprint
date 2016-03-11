﻿using System.Configuration;
using System.Web.Http;
using ServiceLibrary.Helpers;

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

        internal static string LogSourceStatus = "ConfigControl.Status";
        internal static string LogRecordStatus = "ConfigControl.GetLogs";
    }
}
