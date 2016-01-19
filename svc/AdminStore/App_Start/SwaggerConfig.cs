using System.Web.Hosting;
using System.Web.Http;
using Swashbuckle.Application;

namespace AdminStore
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "AdminStore");
                    c.IncludeXmlComments(HostingEnvironment.MapPath("~/bin/AdminStore.XML"));
                    c.IncludeXmlComments(HostingEnvironment.MapPath("~/bin/ServiceLibrary.XML"));
                })
                .EnableSwaggerUi("docs/{*assetPath}");
        }
    }
}
