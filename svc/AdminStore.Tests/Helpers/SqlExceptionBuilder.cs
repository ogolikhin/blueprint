using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AdminStore.Helpers
{
    public class SqlExceptionBuilder
    {
        private int _errorNumber;
        private string _errorMessage;

        public SqlException Build()
        {
            var error = this.CreateError();
            var errorCollection = this.CreateErrorCollection(error);
            var exception = this.CreateException(errorCollection);
            return exception;
        }

        public SqlExceptionBuilder WithErrorNumber(int number)
        {
            this._errorNumber = number;
            return this;
        }

        public SqlExceptionBuilder WithErrorMessage(string message)
        {
            this._errorMessage = message;
            return this;
        }

        private SqlError CreateError()
        {
            var constructors = typeof(SqlError).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var firstSqlErrorConstructor = constructors.FirstOrDefault(
                ctor =>
                ctor.GetParameters().Count() == 7); 
            SqlError error = firstSqlErrorConstructor.Invoke(
                new object[]
                {
                this._errorNumber,
                new byte(),
                new byte(),
                string.Empty,
                string.Empty,
                string.Empty,
                new int()
                }) as SqlError;

            return error;
        }

        private SqlErrorCollection CreateErrorCollection(SqlError error)
        {
            var sqlErrorCollectionConstructors = typeof(SqlErrorCollection).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            SqlErrorCollection errorCollection = sqlErrorCollectionConstructors.Invoke(new object[] { }) as SqlErrorCollection;
            typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(errorCollection, new object[] { error });

            return errorCollection;
        }

        private SqlException CreateException(SqlErrorCollection errorCollection)
        {
            var constructor = typeof(SqlException).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            SqlException sqlException = constructor.Invoke(
                new object[]
                { 
                this._errorMessage,
                errorCollection,
                null,
                Guid.NewGuid()
                }) as SqlException;

            return sqlException;
        }
    }
}
