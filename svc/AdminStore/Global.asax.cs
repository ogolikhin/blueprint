﻿using System.Web.Http;
using ServiceLibrary.Swagger;

namespace AdminStore
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
#if DEBUG
            GlobalConfiguration.Configure(config => SwaggerConfig.Register(config, "~/bin/AdminStore.XML", "AdminStore",
                "AdminStore is Web Service to facilitate application and project administration functionality, user authentication and authorization."));
#endif
        }
    }
}
