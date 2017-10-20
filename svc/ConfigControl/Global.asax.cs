using Logging.Database;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using ServiceLibrary.EventSources;
using ServiceLibrary.LocalLog;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Web.Http;
using ServiceLibrary.Swagger;

namespace ConfigControl
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private const string StartingLoggingListener = "Starting logging listener";
        private const string StartedLoggingListener = "Started logging listener";
        private const string LoggingListenerFailed = "Logging listener failed: {0}";

        private EventListener _dbListener;

        protected void Application_Start()
        {
            this.SetupEventTracing();

            GlobalConfiguration.Configure(WebApiConfig.Register);
#if DEBUG
            GlobalConfiguration.Configure(config => SwaggerConfig.Register(config, "~/bin/ConfigControl.XML", "ConfigControl"));
#endif
        }

        [SuppressMessage("Microsoft.Usage", "CA2215:Dispose methods should call base class dispose", Justification = "base.Dispose() is called from Dispose(bool)")]
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
                localLog.LogInformation(StartingLoggingListener);

                // Log all events to DB
                _dbListener = BlueprintSqlDatabaseLog.CreateListener(
                    "BlueprintSys-Blueprint-Blueprint",
                    WebApiConfig.AdminStorage,
                    bufferingInterval: TimeSpan.FromSeconds(3),
                    bufferingCount: 200);
                _dbListener.EnableEvents(BlueprintEventSource.Log, EventLevel.LogAlways, Keywords.All);
                _dbListener.EnableEvents(CLogEventSource.Log, EventLevel.LogAlways, Keywords.All);
                _dbListener.EnableEvents(StandardLogEventSource.Log, EventLevel.LogAlways, Keywords.All);
                _dbListener.EnableEvents(PerformanceLogEventSource.Log, EventLevel.LogAlways, Keywords.All);
                _dbListener.EnableEvents(SQLTraceLogEventSource.Log, EventLevel.LogAlways, Keywords.All);

                localLog.LogInformation(StartedLoggingListener);
            }
            catch (Exception ex)
            {
                localLog.LogErrorFormat(LoggingListenerFailed, ex.Message);
                throw;
            }
        }

        private void DisposeEventTracing()
        {
            if (_dbListener != null)
            {
                _dbListener.DisableEvents(BlueprintEventSource.Log);
                _dbListener.DisableEvents(CLogEventSource.Log);
                _dbListener.DisableEvents(StandardLogEventSource.Log);
                _dbListener.DisableEvents(PerformanceLogEventSource.Log);
                _dbListener.DisableEvents(SQLTraceLogEventSource.Log);
                _dbListener.Dispose();
            }
        }

    }
}
