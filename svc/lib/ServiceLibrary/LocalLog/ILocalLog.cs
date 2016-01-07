namespace ServiceLibrary.LocalLog
{
    public interface ILocalLog
    {
        void LogError(string message);
        void LogWarning(string message);
        void LogInformation(string message);
    }
}