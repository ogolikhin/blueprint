using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System;


namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    public class ServiceLogRepositoryAdapter : LogListener<StandardLogEntry>
    {
        public static void Initialize(string sourceName, Level minimumLevel = Level.Error)
        {
            LogManager.Manager.AddListener(new ServiceLogRepositoryAdapter(new ServiceLogRepository(), new LogEntryLevelFilter(minimumLevel), sourceName));
        }

        public static void Shutdown()
        {
            LogManager.Manager.ClearListeners();
        }

        private readonly IServiceLogRepository serviceLogRepository;

        public ServiceLogRepositoryAdapter(IServiceLogRepository serviceLogRepository, ILogEntryFilter filter, string name) : base("ServiceLogRepositoryAdapter", name, filter, null)
        {
            this.serviceLogRepository = serviceLogRepository;
        }

        public override void Write(StandardLogEntry entry)
        {
            serviceLogRepository.LogStandardLog(new StandardLogModel() {
                Message = entry.GetContent(),
                UserName = entry.UserName,
                StackTrace = entry.Exception?.StackTrace,
                Source = Name,
                LogLevel = ConvertLogLevel(entry.Level),
                OccurredAt = entry.DateTime,
                SessionId = entry.SessionId
            });
        }

        private LogLevelEnum ConvertLogLevel(Level level)
        {
            switch (level)
            {
                case Level.Debug:
                    return LogLevelEnum.Verbose;

                case Level.Info:
                    return LogLevelEnum.Informational;

                case Level.Warn:
                    return LogLevelEnum.Warning;

                case Level.Error:
                    return LogLevelEnum.Error;

                case Level.Fatal:
                    return LogLevelEnum.Critical;

                default:
                    throw new ArgumentException("Unknown log level: " + level.ToString(), nameof(level));
            }
        }
    }
}
