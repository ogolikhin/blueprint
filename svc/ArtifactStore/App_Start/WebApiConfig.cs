using System.Configuration;
using System.Net.Http;
using System.Web.Http;
using System.Web.Routing;

namespace ArtifactStore
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            RouteTable.Routes.MapHttpRoute(
            name: "CollectionsApi",
            defaults: new { controller = "Collections", action = "AddArtifactsToCollectionAsync" },
            constraints: new { httpMethod = new HttpMethodConstraint("POST") },
            routeTemplate: "svc/artifactstore/collections/{id}/artifacts?{operation}");

            // Web API routes
            config.MapHttpAttributeRoutes();

        }

        public static string ArtifactStorage = ConfigurationManager.ConnectionStrings["ArtifactStorage"].ConnectionString;

        public static string StatusCheckPreauthorizedKey = ConfigurationManager.AppSettings["StatusCheckPreauthorizedKey"];

        internal static string LogSourceStatus = "ArtifactStore.Status";
    }
}
