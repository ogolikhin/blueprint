using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class LogEntry : ILogEntry
    {

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
