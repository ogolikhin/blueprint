namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    /// <summary>
    /// Log listener that writes to a file
    /// </summary>
    public interface IFileLogListener : ILogListener
    {
        string File { get; }
    }
}