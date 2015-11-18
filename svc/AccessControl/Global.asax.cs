using System;
using System.Web;
using System.Web.Http;
using ServiceLibrary.Log;

namespace AccessControl
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            LogProvider.Init(new EventLogProviderImpl(WebApiConfig.ServiceLogSource, WebApiConfig.ServiceLogName));
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        public sealed override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose();
                LogProvider.DisposeCurrent();
            }
        }
    }
}
