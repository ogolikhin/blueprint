using System;
using System.Web;

namespace ArtifactStore.Helpers
{
    public class RemoveSecurityRelatedHeadersHttpModuleIHttpModule
    {
public const string ServerName = "Blueprint";

        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            if (HttpContext.Current == null)
            {
                return;
            }

#if !DEBUG
            // Replace IIS version with a generic name
            HttpContext.Current.Response.Headers.Set("Server", ServerName);
            // This is being done here just in case, but we are also doing this in the web.config by having enableVersionHeader="false"
            HttpContext.Current.Response.Headers.Remove("X-AspNet-Version");

            // Fix potential security issues. https://www.owasp.org/index.php/List_of_useful_HTTP_headers
            HttpContext.Current.Response.Headers.Set("X-XSS-Protection", "1; mode=block");
            HttpContext.Current.Response.Headers.Set("X-Content-Type-Options", "nosniff");
            // Note, we are also removing the X-Powered-By.  This is being done by the httpProtocol section in web.config
#endif
        }

        public void Dispose() { }
    }
}