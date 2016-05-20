using ServiceLibrary.Repositories.ConfigControl;

namespace ServiceLibrary.Helpers
{
    public interface ILoggable
    {
        IServiceLogRepository Log { get; }

        string LogSource { get; }
    }
}