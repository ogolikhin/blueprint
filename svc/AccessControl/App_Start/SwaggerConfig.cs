using System.Web.Hosting;
using System.Web.Http;
using Swashbuckle.Application;

namespace AccessControl
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "AccessControl");
                    c.IncludeXmlComments(HostingEnvironment.MapPath("~/bin/AccessControl.XML"));
                    c.IncludeXmlComments(HostingEnvironment.MapPath("~/bin/ServiceLibrary.XML"));
                })
                .EnableSwaggerUi("docs/{*assetPath}");
        }
    }
}
