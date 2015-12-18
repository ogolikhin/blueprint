namespace ServiceLibrary.LocalEventLog
{
    public interface ILocalLog
    {
        void LogError(string message);
        void LogWarning(string message);
    }
}