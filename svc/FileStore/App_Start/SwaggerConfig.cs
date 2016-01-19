using System.Web.Hosting;
using System.Web.Http;
using Swashbuckle.Application;

namespace FileStore
{
    public class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "FileStore");
                    c.IncludeXmlComments(HostingEnvironment.MapPath("~/bin/FileStore.XML"));
                    c.IncludeXmlComments(HostingEnvironment.MapPath("~/bin/ServiceLibrary.XML"));
                })
                .EnableSwaggerUi("docs/{*assetPath}");
        }
    }
}
