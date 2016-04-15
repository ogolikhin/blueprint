namespace ServiceLibrary.Repositories.ConfigControl
{
    public interface ILoggable
    {
        IServiceLogRepository Log { get; }

        string LogSource { get; }
    }
}