using System;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace Model.Impl
{
    public class MsSqlDatabase : IDatabase
    {
        private bool _disposed = false;
        private SqlConnection _sqlConnection;

        public string ConnectionString { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connectionString">The connection string to the MS SQL database.</param>
        public MsSqlDatabase(string connectionString)
        {
            ConnectionString = connectionString;
            _sqlConnection = new SqlConnection(ConnectionString);
        }

        ~MsSqlDatabase()
        {
            Dispose(false);
        }

        #region IDisposable members

        /// <summary>
        /// Disposes this object's resources properly.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this object's resources properly.
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the objects or false to just set them to null.  Only the destructor should pass false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_sqlConnection != null) { _sqlConnection.Dispose(); }
                }

                _sqlConnection = null;
            }

            _disposed = true;
        }

        #endregion IDisposable members

        #region IDatabase members

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        public void Close()
        {
            _sqlConnection.Close();
        }

        /// <summary>
        /// Creates and returns a SqlCommand object associated with the SqlConnection.  The caller is responsible for disposing the SqlCommand that is returned.
        /// </summary>
        /// <returns>A SqlCommand object associated with the SqlConnection.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]   // Ignore this warning.
        public SqlCommand CreateSqlCommand(string sqlCommand)
        {
            SqlCommand cmd = new SqlCommand(sqlCommand, _sqlConnection);
            return cmd;
        }

        /// <summary>
        /// Opens a database connection with the property settings specified by the ConnectionString.
        /// </summary>
        public void Open()
        {
            _sqlConnection.Open();
        }

        #endregion IDatabase members
    }

    [Serializable]
    public class SqlQueryFailedException : Exception
    {
        public SqlQueryFailedException()
        { }

        public SqlQueryFailedException(string msg)
            : base(msg)
        { }

        public SqlQueryFailedException(string msg, Exception e)
            : base(msg, e)
        { }

        protected SqlQueryFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

}
