/// *************************************************************************************
/// ***** Any changes to this file need to be replicated in the                     *****
/// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
/// *************************************************************************************
namespace ServiceLibrary.LocalLog
{
    public interface ILocalLog
    {
        void LogError(string message);
        void LogErrorFormat(string format, params object[] args);
        void LogWarning(string message);
        void LogWarningFormat(string format, params object[] args);
        void LogInformation(string message);
        void LogInformationFormat(string format, params object[] args);
    }
}
