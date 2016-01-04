namespace ServiceLibrary.Models
{
    public interface ILogEntry
    {
        string FilePath { get; set; }
        int LineNumber { get; set; }
        LogLevelEnum LogLevel { get; set; }
        string Message { get; set; }
        string MethodName { get; set; }
        string Source { get; set; }
        string StackTrace { get; set; }
    }
}