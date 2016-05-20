﻿using System.Configuration;
using System.Web.Http;

namespace ArtifactStore
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
        }

        public static string ArtifactStorage = ConfigurationManager.ConnectionStrings["ArtifactStorage"].ConnectionString;

        internal static string LogSourceStatus = "ArtifactStore.Status";
    }
}
