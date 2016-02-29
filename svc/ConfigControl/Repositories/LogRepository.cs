using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Dapper;
using ServiceLibrary.Repositories;

namespace ConfigControl.Repositories
{
    public class LogRepository : ILogRepository
    {
        internal readonly ISqlConnectionWrapper ConnectionWrapper;
        private const string defaultStoredProcedure = "GetLogs";
        private string csvDelemiter = ",";

        public LogRepository() : this(new SqlConnectionWrapper(WebApiConfig.AdminStorage))
        {
        }

        internal LogRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        private string GetSingleEntry(IDataReader reader, bool nameOnly = false)
        {
            var line = new StringBuilder();
            // column loop
            for (var fieldCounter = 0; fieldCounter < reader.FieldCount; fieldCounter++)
            {
                line.AppendFormat("{0}{1}", nameOnly ? reader.GetName(fieldCounter) : reader.GetValue(fieldCounter), csvDelemiter);
            }
            line.Length--;
            return line.ToString();

        }
        public IEnumerable<string> GetLogEntries(int numberOfRecords, bool showHeader)
        {

            DbConnection dbConnection = null;
            try
            {
                dbConnection = ConnectionWrapper.CreateConnection();
                dbConnection.Open();


                var prm = new DynamicParameters();
                prm.Add("@limit", numberOfRecords);
                var reader = dbConnection.ExecuteReader(defaultStoredProcedure, prm,
                    commandType: CommandType.StoredProcedure);

                if (showHeader)
                    yield return GetSingleEntry(reader, true);

                while (reader.Read())
                {
                    yield return GetSingleEntry(reader);
                }

            }
            finally
            {
                dbConnection?.Close();
            }
        }
    }
}