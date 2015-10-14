using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;

namespace FileStore
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
        }

        public static string FileStoreDatabase = ConfigurationManager.ConnectionStrings["FileStoreDatabase"].ConnectionString;
        public static string FileStreamDatabase = ConfigurationManager.ConnectionStrings["FileStreamDatabase"].ConnectionString;

    }
}
