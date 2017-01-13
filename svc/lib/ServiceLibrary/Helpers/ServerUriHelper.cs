using System;
using System.Web;

namespace ServiceLibrary.Helpers
{
    public static class ServerUriHelper
    {
        public static Uri BaseHostUri
        {
            get
            {
                var hostUri = HttpContext.Current.Request.Url;
                var builder = new UriBuilder();
                builder.Scheme = hostUri.Scheme;
                builder.Host = hostUri.Host;
                builder.Port = hostUri.Port;
                return builder.Uri;
            }
        }
    }
}
