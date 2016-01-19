using System.Web.Hosting;
using System.Web.Http;
using Swashbuckle.Application;

namespace ArtifactStore
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "ArtifactStore");
                    c.IncludeXmlComments(HostingEnvironment.MapPath("~/bin/ArtifactStore.XML"));
                    c.IncludeXmlComments(HostingEnvironment.MapPath("~/bin/ServiceLibrary.XML"));
                })
                .EnableSwaggerUi("docs/{*assetPath}");
        }
    }
}
