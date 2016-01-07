using System;

namespace ServiceLibrary.Models
{
    public class CLogEntry : LogEntry
    {
        public string TimeZoneOffset { get; set; }
        public DateTime OccuredAt { get; set; }
        public string UserName { get; set; }
        public string ActionName { get; set; }
        public double TotalDuration { get; set; }
    }
}
