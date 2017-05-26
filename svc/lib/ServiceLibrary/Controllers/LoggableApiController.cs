using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace ServiceLibrary.Controllers
{
    public abstract class LoggableApiController : BaseApiController, ILoggable
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

    public abstract class SessionRequiredLoggableApiController : LoggableApiController
    {
        public Session CurrentSession => Request.Properties[ServiceConstants.SessionProperty] as Session;
    }
}
