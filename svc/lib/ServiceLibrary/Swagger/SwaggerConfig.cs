using System.Web.Hosting;
using System.Web.Http;
using Swashbuckle.Application;

namespace ServiceLibrary.Swagger
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config, string xmlCommentsPath, string title, string description = null)
        {
            config
                .EnableSwagger(c =>
                {
                    c.UseFullTypeNameInSchemaIds();
                    c.SingleApiVersion("v1", title).Description(description);
                    c.ApiKey("apiKey").Description("API Key Authentication").Name("apiKey").In("header");
                    c.IncludeXmlComments(HostingEnvironment.MapPath(xmlCommentsPath));
                    c.IncludeXmlComments(HostingEnvironment.MapPath("~/bin/ServiceLibrary.XML"));
                })
                .EnableSwaggerUi("docs/{*assetPath}", c =>
                {
                    c.InjectJavaScript(typeof(SwaggerConfig).Assembly, "ServiceLibrary.Swagger.swagger-config.js");
                });
        }
    }
}
