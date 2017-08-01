using Dapper;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace ConfigControl.Repositories
{
    public class LogRepository : ILogRepository
    {
        private const string defaultStoredProcedure = "[AdminStore].GetLogs";
        private const string csvDelemiter = ",";

        private readonly ISqlConnectionWrapper _connectionWrapper;
        private DbConnection dbConnection;

        public LogRepository() : this(new SqlConnectionWrapper(WebApiConfig.AdminStorage))
        {
        }

        internal LogRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        private LogRecord GetSingleEntry(IDataReader reader, bool nameOnly = false)
        {
            var result = new LogRecord(); 
            var line = new StringBuilder();
            // column loop
            for (var fieldCounter = 0; fieldCounter < reader.FieldCount; fieldCounter++)
            {
                object csvValue;
                if (nameOnly)
                    csvValue = reader.GetName(fieldCounter);
                else
                {
                    if (fieldCounter == 0) {
                        result.Id = (long)reader.GetValue(fieldCounter);
                    }
                    var value = reader.GetValue(fieldCounter);
                    csvValue = value is string ? string.Concat("\"", value.ToString().Replace("\"","'"),"\"") : value;
                }
                line.AppendFormat("{0}{1}", csvValue, csvDelemiter);
            }
            line.Length--;

            result.Line = line.ToString();
            return result;

        }

        
        public IEnumerable<LogRecord> GetRecords(int numberOfRecords, long? recordId, bool showHeader = false)
        {
            if (dbConnection == null)
            {
                dbConnection = _connectionWrapper.CreateConnection();
            }
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }
            var prm = new DynamicParameters();
            prm.Add("@recordlimit", numberOfRecords);
            prm.Add("@recordid", recordId);

            var reader = dbConnection.ExecuteReader(defaultStoredProcedure, prm, commandType: CommandType.StoredProcedure);

            if (showHeader)
            {
                yield return GetSingleEntry(reader, true);
            }

            while (reader.Read())
            {
                yield return GetSingleEntry(reader);
            }
        }

        public void Close()
        {
            dbConnection?.Close();
        }

    }
}