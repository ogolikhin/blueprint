using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class LogEntry
    {
        private LogEntry()
        {
            //disable
        }

        public LogEntry(
            LogLevelEnum logLevel,
            string source,
            string message,
            string methodName,
            string filePath,
            int lineNumber,
            string stackTrace)
        {
            LogLevel = logLevel;
            Source = source == null ? string.Empty : source;
            Message = message == null ? string.Empty: message;
            MethodName = methodName==null ? string.Empty: methodName;
            FilePath = filePath == null ? string.Empty : filePath;
            LineNumber = lineNumber;
            StackTrace = stackTrace == null ? string.Empty : stackTrace;
        }

        [JsonProperty]
        public string Source { get; set; }

        [JsonProperty]
        public LogLevelEnum LogLevel { get; set; }

        [JsonProperty]
        public string Message { get; set; }

        [JsonProperty]
        public string MethodName { get; set; }

        [JsonProperty()]
        public string FilePath { get; set; }

        [JsonProperty]
        public int LineNumber { get; set; }

        [JsonProperty]
        public string StackTrace { get; set; }
    }
}
