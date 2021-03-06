﻿using ServiceLibrary.Helpers;
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
}
