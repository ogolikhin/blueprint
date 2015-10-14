using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data.SqlTypes;
 
namespace FileStore.Repositories
{
	public class FileStreamRepository : IFileStreamRepository
	{
 
        public async Task<byte[]> GetTSqlFileStreamAsync(Guid guid)
        {
            // For best performance and optimum usage of the SQL Server resources, 
            // FILESTREAM data should not generally be accessed using TSQL. This method
            // provides a fallback if there are issues with the streaming APIs

            byte[] buffer = null;

            using (var db = new SqlConnection(WebApiConfig.FileStreamDatabase))
            {
                await db.OpenAsync();

                using (var sqlCommand = new SqlCommand())
                {
                    sqlCommand.Connection = db;

                    sqlCommand.CommandText =
                        String.Format("SELECT [Content] FROM Files WHERE(FileGuid = '{0}')", guid.ToString());

                    var result = await sqlCommand.ExecuteScalarAsync();

                    buffer = result != null ? result as byte[] : null;
                }
            }
            return buffer;

        }

        public async Task<byte[]> GetFileStreamAsync(Guid guid)
        {
            // This method uses the streaming APIs exposed by SQL Server
            // to retrieve a FILESTREAM data file 

            byte[] buffer = null;

            using (var db = new SqlConnection(WebApiConfig.FileStreamDatabase))
            {
                await db.OpenAsync();

                // Retrieve the PathName() and transaction context
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = db;

                    cmd.CommandText = String.Format("SELECT [Content].PathName() AS filePath, " +
                        "GET_FILESTREAM_TRANSACTION_CONTEXT() AS txContext " +
                        "FROM Files WHERE ( FileGuid = '{0}' )", guid);

                    // Begin a transaction
                    using (SqlTransaction trn = db.BeginTransaction("fsTransaction"))
                    {
                        try
                        {
                            cmd.Transaction = trn;

                            // Execute the query
                            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    // Open and read file using SqlFileStream Class
                                    string filePath = reader.GetString(0);
                                    object objContext = reader.GetValue(1);
                                    byte[] txContext = (byte[])objContext;

                                    using (SqlFileStream fs = new SqlFileStream(
                                            filePath, txContext, System.IO.FileAccess.Read))
                                    {
                                        if (fs != null)
                                        {
                                            buffer = new byte[(int)fs.Length];
                                            fs.Read(buffer, 0, (int)fs.Length);
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            trn.Rollback();
                            throw;
                        }
                    }
                }
            }

            return buffer;
        }
    }
}