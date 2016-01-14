using System;
using System.Configuration;
using System.Web.Http;

namespace AccessControlDouble
{
    public static class WebApiConfig
    {
        public const string SVC_PATH = "/svc/accesscontrol/";

        public static readonly string AccessControl = ConfigurationManager.AppSettings["AccessControl"]?.TrimEnd('/');
        public static readonly string AdminStore = ConfigurationManager.AppSettings["AdminStore"]?.TrimEnd('/');
        public static readonly string LogFile = ConfigurationManager.AppSettings["LogFile"]?.TrimEnd('\\');

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
