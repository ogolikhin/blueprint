using System.IO;
using System.Collections.Generic;

namespace ConfigControl.Repositories
{

    public struct LogRecord
    {
        public long Id { get; set; }
        public string Line { get; set; }
    }

    public interface ILogRepository
    {
        IEnumerable<LogRecord> GetRecords(int numberOfRecords, long? recordId, bool showHeader = false);
        void Close();
    }

}