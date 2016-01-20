using System.Web.Hosting;
using System.Web.Http;
using Swashbuckle.Application;

namespace ConfigControl
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "ConfigControl");
                    c.IncludeXmlComments(HostingEnvironment.MapPath("~/bin/ConfigControl.XML"));
                    c.IncludeXmlComments(HostingEnvironment.MapPath("~/bin/ServiceLibrary.XML"));
                })
                .EnableSwaggerUi("docs/{*assetPath}");
        }
    }
}
