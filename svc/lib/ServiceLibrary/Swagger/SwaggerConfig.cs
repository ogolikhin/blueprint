using System.Web.Hosting;
using System.Web.Http;
using Swashbuckle.Application;

namespace ServiceLibrary.Swagger
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config, string title, string xmlCommentsPath)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", title);
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
