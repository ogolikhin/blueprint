using System.Web.Http;
using ServiceLibrary.Repositories.ConfigControl;

namespace ServiceLibrary.Helpers
{
    public abstract class LoggableApiController : ApiController, ILoggable
    {
        protected LoggableApiController() : this(new ServiceLogRepository())
        {
        }

        protected LoggableApiController(IServiceLogRepository log)
        {
            Log = log;
        }

        public IServiceLogRepository Log { get; }

        public abstract string LogSource { get; }
    }
}
