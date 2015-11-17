using System.Web.Http;
using ServiceLibrary.Log;

namespace AccessControl
{
    public class WebApiApplication : System.Web.HttpApplication
    {        
        protected void Application_Start()
        {
            LogProvider.Init(new EventLogProviderImpl(WebApiConfig.ServiceLogSource, WebApiConfig.ServiceLogName));
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        public override void Dispose()
        {
            LogProvider.DisposeCurrent();
            base.Dispose();            
        }
    }
}
