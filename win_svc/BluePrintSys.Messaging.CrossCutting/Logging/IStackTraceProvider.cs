namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    public interface IStackTraceProvider
    {
        string GetStackTrace(int skipFrames);
    }
}
