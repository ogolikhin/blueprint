using System.Collections.Generic;

namespace ConfigControl.Repositories
{
    public interface ILogRepository
    {
        IEnumerable<string> GetLogEntries(int numberOfRecords, bool showHeader);
    }

}