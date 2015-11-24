using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.WebHost;

namespace FileStore
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHostBufferPolicySelector), new StreamingBufferPolicySelector());

            // Web API routes
            config.MapHttpAttributeRoutes();
        }
    }

    public class StreamingBufferPolicySelector : WebHostBufferPolicySelector
    {
        public override bool UseBufferedInputStream(object hostContext)
        {
            return false;
        }

        public override bool UseBufferedOutputStream(HttpResponseMessage response)
        {
            return false;
        }
    }
}
