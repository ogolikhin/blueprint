/// *************************************************************************************
/// ***** Any changes to this file need to be replicated in the                     *****
/// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
/// *************************************************************************************
namespace ServiceLibrary.LocalLog
{
    public interface ILocalLog
    {
        void LogError(string message);
        void LogWarning(string message);
        void LogInformation(string message);
    }
}