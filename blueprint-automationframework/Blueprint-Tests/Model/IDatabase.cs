using System;
using System.Data.SqlClient;

namespace Model
{
    public interface IDatabase : IDisposable
    {
        string ConnectionString { get; set; }


        /// <summary>
        /// Closes the database connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Creates and returns a SqlCommand object associated with the SqlConnection.  The caller is responsible for disposing the SqlCommand that is returned.
        /// </summary>
        /// <returns>A SqlCommand object associated with the SqlConnection.</returns>
        SqlCommand CreateSqlCommand(string sqlCommand);

        /// <summary>
        /// Opens a database connection with the property settings specified by the ConnectionString.
        /// </summary>
        void Open();
    }
}
