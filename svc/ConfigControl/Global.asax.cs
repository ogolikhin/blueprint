using Logging.Database;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using ServiceLibrary.EventSources;
using ServiceLibrary.LocalLog;
using System;
using System.Diagnostics.Tracing;
using System.Web.Http;

namespace ConfigControl
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private EventListener dbListener;

        protected void Application_Start()
        {
            this.SetupEventTracing();

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
                this.DisposeEventTracing();
            }
        }

        private void SetupEventTracing()
        {
            ILocalLog localLog = new LocalFileLog();

            try
            {
                localLog.LogInformation("Starting logging listener");

                // Log all events to DB 
                this.dbListener = BlueprintSqlDatabaseLog.CreateListener(
                    "BlueprintSys-Blueprint-Blueprint",
                    WebApiConfig.AdminStorage,
                    bufferingInterval: TimeSpan.FromSeconds(3),
                    bufferingCount: 200);
                dbListener.EnableEvents(BlueprintEventSource.Log, EventLevel.LogAlways, Keywords.All);
                dbListener.EnableEvents(CLogEventSource.Log, EventLevel.LogAlways, Keywords.All);
                dbListener.EnableEvents(StandardLogEventSource.Log, EventLevel.LogAlways, Keywords.All);
                dbListener.EnableEvents(PerformanceLogEventSource.Log, EventLevel.LogAlways, Keywords.All);
                dbListener.EnableEvents(SQLTraceLogEventSource.Log, EventLevel.LogAlways, Keywords.All);

                localLog.LogInformation("Started logging listener");
            }
            catch (Exception ex)
            {
                localLog.LogError(string.Format("Logging listener failed: {0}", ex.Message));
                throw;
            }
        }

        private void DisposeEventTracing()
        {
            if (dbListener != null)
            {
                this.dbListener.DisableEvents(BlueprintEventSource.Log);
                this.dbListener.Dispose();
            }
        }

    }
}
