using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web.Http;

namespace AccessControlDouble
{
    public static class WebApiConfig
    {
        public const string SVC_PATH = "/svc/accesscontrol/";

        public static readonly string AccessControl = ConfigurationManager.AppSettings["AccessControl"]?.TrimEnd('/');
        public static readonly string AdminStore = ConfigurationManager.AppSettings["AdminStore"]?.TrimEnd('/');
        public static readonly string LogFile = ConfigurationManager.AppSettings["LogFile"]?.TrimEnd('\\');

        /// <summary>
        /// This is a map of statuses to return for each request type.  A null value means to return the actual value from AccessControl.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")] // Ignore this warning.
        public static Dictionary<string, HttpStatusCode?> StatusCodeToReturn { get; } = new Dictionary<string, HttpStatusCode?>
        {
            { "DELETE", null },
            { "GET", null },
            { "HEAD", null },
            { "POST", null },
            { "PUT", null }
        };

        public static void Register(HttpConfiguration config)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "svc/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
