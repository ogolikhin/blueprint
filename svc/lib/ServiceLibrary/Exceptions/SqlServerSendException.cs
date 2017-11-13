using System;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Exceptions
{
    [Serializable]
    public class SqlServerSendException : ExceptionWithErrorCode
    {
        public SqlServerSendException(Exception ex) : base("The message could not be sent because SQL Server could not be reached. Please check the connection settings. " + ex.Message, ErrorCodes.SqlServerSend)
        {
        }
    }
}
